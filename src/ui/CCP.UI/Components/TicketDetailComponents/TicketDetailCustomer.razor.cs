using CCP.Shared.UIContext;
using CCP.UI.Services;
using IdentityService.Sdk.Services.User;
using MessagingService.Sdk.Dtos;
using MessagingService.Sdk.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using TicketService.Sdk.Dtos;

namespace CCP.UI.Pages.Tickets.TicketDetailComponents;

public partial class TicketDetailCustomer : ComponentBase, IAsyncDisposable
{
    [Inject] private ChatHubService HubService { get; set; } = default!;
    [Inject] private IMessageSdkService MessageSdkService { get; set; } = default!;
    [Inject] private IUIUserContext UserContext { get; set; } = default!;
    [Inject] private IUserService UserService { get; set; } = default!;
    [Inject] private ILogger<TicketDetailCustomer> Logger { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    [Parameter, EditorRequired] public TicketSdkDto Ticket { get; set; } = default!;

    private List<MessageDto> _messages = new();
    private string _newMessageContent = string.Empty;
    private bool _isSending;
    private bool _isLoadingMessages = true;
    private readonly Dictionary<Guid, string> _userNameCache = new();
    private string? _customerName;

    // Pending attachment state
    private AttachmentDto? _pendingAttachment;
    private string? _pendingAttachmentPreviewUrl;
    private bool _isUploadingAttachment;

    // Lightbox state
    private string? _lightboxUrl;
    private string? _lightboxFileName;
    private string? _lightboxContentType;

    // Pagination state
    private bool _hasMoreMessages;
    private bool _isLoadingMoreMessages;
    private bool _shouldScrollToBottom;
    private ElementReference _messagesContainer;

    private const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50 MB

    private void OpenLightbox(string url, string? fileName, string? contentType = null)
    {
        _lightboxUrl = url;
        _lightboxFileName = fileName;
        _lightboxContentType = contentType;
        StateHasChanged();
    }

    private void CloseLightbox()
    {
        _lightboxUrl = null;
        _lightboxFileName = null;
        _lightboxContentType = null;
        StateHasChanged();
    }

    protected override async Task OnInitializedAsync()
    {
        if (!RendererInfo.IsInteractive)
            return;

        HubService.OnMessageReceived += HandleMessageReceived;
        HubService.OnMessageUpdated += HandleMessageUpdated;
        HubService.OnMessageDeleted += HandleMessageDeleted;

        await LoadMessagesAsync();
        _ = ConnectHubAsync();
    }

    private async Task LoadMessagesAsync()
    {
        _isLoadingMessages = true;

        var result = await MessageSdkService.GetMessagesByTicketIdAsync(Ticket.Id, limit: 15);

        if (result.IsSuccess && result.Value is not null)
        {
            // Customers never see internal notes
            _messages = result.Value.Items.Where(m => !m.IsInternalNote).ToList();
            await ResolveUserNamesAsync(_messages);
            if (Ticket.CustomerId.HasValue && _userNameCache.TryGetValue(Ticket.CustomerId.Value, out var customerName))
                _customerName = customerName;
            _hasMoreMessages = result.Value.HasMore;
        }
        else
        {
            Logger.LogError("TicketDetailCustomer failed to load messages for ticket {TicketId}: {Error}", Ticket.Id, result.Error);
        }

        _isLoadingMessages = false;
        _shouldScrollToBottom = true;
        await InvokeAsync(StateHasChanged);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_shouldScrollToBottom)
        {
            _shouldScrollToBottom = false;
            await ScrollToBottomAsync();
        }
    }

    private async Task ScrollToBottomAsync()
    {
        try { await JSRuntime.InvokeVoidAsync("scrollHelpers.scrollToBottom", _messagesContainer); }
        catch { /* ignore if JS not ready */ }
    }

    private async Task LoadMoreMessagesAsync()
    {
        if (_isLoadingMoreMessages || !_hasMoreMessages || !_messages.Any()) return;
        _isLoadingMoreMessages = true;
        await InvokeAsync(StateHasChanged);

        var beforeId = _messages.Min(m => m.Id);
        double previousScrollHeight = 0;
        try { previousScrollHeight = await JSRuntime.InvokeAsync<double>("scrollHelpers.getScrollHeight", _messagesContainer); }
        catch { }

        var result = await MessageSdkService.GetMessagesByTicketIdAsync(Ticket.Id, 50, beforeId);
        if (result.IsSuccess && result.Value.Items.Any())
        {
            var olderMessages = result.Value.Items.Where(m => !m.IsInternalNote).ToList();
            await ResolveUserNamesAsync(olderMessages);
            _messages.InsertRange(0, olderMessages);
            _hasMoreMessages = result.Value.HasMore;
            await InvokeAsync(StateHasChanged);
            try { await JSRuntime.InvokeVoidAsync("scrollHelpers.preserveScrollPosition", _messagesContainer, previousScrollHeight); }
            catch { }
        }
        else
        {
            _hasMoreMessages = false;
        }

        _isLoadingMoreMessages = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task OnScrollAsync()
    {
        if (_isLoadingMoreMessages || !_hasMoreMessages) return;
        double scrollTop = 0;
        try { scrollTop = await JSRuntime.InvokeAsync<double>("scrollHelpers.getScrollTop", _messagesContainer); }
        catch { return; }
        if (scrollTop <= 50)
            await LoadMoreMessagesAsync();
    }

    private async Task ConnectHubAsync()
    {
        try { await HubService.StartAsync(); }
        catch (Exception ex)
        {
            Logger.LogError(ex, "TicketDetailCustomer failed to start hub");
            return;
        }

        var waited = 0;
        while (!HubService.IsConnected && waited < 5000) { await Task.Delay(100); waited += 100; }

        if (HubService.IsConnected)
            _ = HubService.JoinTicketGroupAsync(Ticket.Id);
    }

    private async Task HandleFileSelected(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file is null) return;

        if (file.Size > MaxFileSizeBytes)
        {
            Logger.LogWarning("File {FileName} exceeds 50MB limit", file.Name);
            _isUploadingAttachment = false;
            await InvokeAsync(StateHasChanged);
            return;
        }

        _isUploadingAttachment = true;
        await InvokeAsync(StateHasChanged);

        try
        {
            await using var stream = file.OpenReadStream(maxAllowedSize: 50 * 1024 * 1024);
            var result = await MessageSdkService.UploadAttachmentAsync(stream, file.Name, file.ContentType);

            if (result.IsSuccess)
            {
                _pendingAttachment = result.Value;
                _pendingAttachmentPreviewUrl = file.ContentType.StartsWith("image/") ? _pendingAttachment.Url : null;
            }
            else
            {
                Logger.LogError("Attachment upload failed: {Code} - {Description}", result.Error.Code, result.Error.Description);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception while uploading attachment");
        }

        _isUploadingAttachment = false;
        await InvokeAsync(StateHasChanged);
    }

    private void RemovePendingAttachment()
    {
        _pendingAttachment = null;
        _pendingAttachmentPreviewUrl = null;
    }

    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(_newMessageContent) && _pendingAttachment is null)
            return;
        if (_isSending) return;

        _isSending = true;

        var result = await MessageSdkService.CreateMessageAsync(
            ticketId: Ticket.Id,
            organizationId: Ticket.OrganizationId,
            userId: UserContext.UserId,
            content: _newMessageContent,
            isInternalNote: false,
            attachmentUrl: _pendingAttachment?.Url,
            attachmentFileName: _pendingAttachment?.FileName,
            attachmentContentType: _pendingAttachment?.ContentType);

        if (result.IsSuccess)
        {
            _newMessageContent = string.Empty;
            _pendingAttachment = null;
            _pendingAttachmentPreviewUrl = null;
            _shouldScrollToBottom = true;
        }
        else
        {
            Logger.LogError("TicketDetailCustomer failed to send message: {Code} - {Description}",
                result.Error.Code, result.Error.Description);
        }

        _isSending = false;
        await InvokeAsync(StateHasChanged);
    }

    private void HandleMessageReceived(MessageDto message)
    {
        if (message.TicketId != Ticket.Id || _messages.Any(m => m.Id == message.Id)) return;
        _messages.Add(message);
        _ = ResolveUserNamesAsync(new[] { message })
            .ContinueWith(_ => InvokeAsync(async () =>
            {
                _shouldScrollToBottom = true;
                StateHasChanged();
            }));
    }

    private void HandleMessageUpdated(MessageDto message)
    {
        var index = _messages.FindIndex(m => m.Id == message.Id);
        if (index >= 0) { _messages[index] = message; InvokeAsync(StateHasChanged); }
    }

    private void HandleMessageDeleted(int messageId)
    {
        var message = _messages.FirstOrDefault(m => m.Id == messageId);
        if (message is not null) { message.IsDeleted = true; message.Content = "[deleted]"; InvokeAsync(StateHasChanged); }
    }

    private async Task ResolveUserNamesAsync(IEnumerable<MessageDto> messages)
    {
        if (!_userNameCache.ContainsKey(UserContext.UserId))
            _userNameCache[UserContext.UserId] = UserContext.FullName;

        var unknownIds = messages
            .Where(m => m.UserId.HasValue && m.UserId.Value != Guid.Empty)
            .Select(m => m.UserId!.Value)
            .Distinct()
            .Where(id => !_userNameCache.ContainsKey(id))
            .ToList();

        var tasks = unknownIds.Select(async userId =>
        {
            try
            {
                var r = await UserService.GetUserDetailsAsync(userId);
                return (userId, name: r.IsSuccess ? r.Value.name : (string?)null);
            }
            catch { return (userId, name: (string?)null); }
        });

        foreach (var (userId, name) in await Task.WhenAll(tasks))
            if (name is not null) _userNameCache[userId] = name;
    }

    private void NavigateBack() => NavigationManager.NavigateTo("/inbox");

    private bool IsOwnMessage(MessageDto m) => UserContext.UserId != Guid.Empty && m.UserId == UserContext.UserId;

    private string GetDisplayName(Guid? userId)
    {
        if (userId is null) return "Unknown";
        if (userId == UserContext.UserId) return "You";
        if (_userNameCache.TryGetValue(userId.Value, out var name)) return name;
        return _customerName ?? "Unknown";
    }

    private string GetInitials(Guid? userId)
    {
        if (userId is null) return "?";
        if (userId == UserContext.UserId) return UserContext.Initials;
        if (_userNameCache.TryGetValue(userId.Value, out var name))
        {
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2) return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            return name.Length > 0 ? name[0].ToString().ToUpper() : "?";
        }
        return "?";
    }

    private async Task HandleKeyDown(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey) await SendMessageAsync();
    }

    private string GetStatusLabel(int status) => status switch
    {
        0 => "Open",
        1 => "Waiting for customer",
        2 => "Waiting for support",
        3 => "Closed",
        4 => "Blocked",
        _ => "Unknown"
    };

    private string GetStatusTagClass(int status) => status switch
    {
        0 => "tdc-tag-teal",
        1 => "tdc-tag-amber",
        2 => "tdc-tag-indigo",
        3 => "tdc-tag-slate",
        4 => "tdc-tag-red",
        _ => "tdc-tag-slate"
    };

    public async ValueTask DisposeAsync()
    {
        HubService.OnMessageReceived -= HandleMessageReceived;
        HubService.OnMessageUpdated -= HandleMessageUpdated;
        HubService.OnMessageDeleted -= HandleMessageDeleted;
        await HubService.DisposeAsync();
    }
}
