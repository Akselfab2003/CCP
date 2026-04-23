using Gateway.Sdk.Services;
using CCP.Shared.UIContext;
using CCP.Shared.ValueObjects;
using CCP.UI.Services;
using IdentityService.Sdk.Services.User;
using MessagingService.Sdk.Dtos;
using MessagingService.Sdk.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using TicketService.Sdk.Dtos;
using TicketService.Sdk.Services.Ticket;

namespace CCP.UI.Pages.Tickets.TicketDetailComponents;

public partial class TicketDetailSupporter : ComponentBase, IAsyncDisposable
{
    [Inject] private ChatHubService HubService { get; set; } = default!;
    [Inject] private IMessageSdkService MessageSdkService { get; set; } = default!;
    [Inject] private IUIUserContext UserContext { get; set; } = default!;
    [Inject] private ITicketService TicketService { get; set; } = default!;
    [Inject] private IUserService UserService { get; set; } = default!;
    [Inject] private IGatewayService GatewayService { get; set; } = default!;
    [Inject] private ILogger<TicketDetailSupporter> Logger { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    [Parameter, EditorRequired] public TicketSdkDto Ticket { get; set; } = default!;

    private List<MessageDto> _messages = new();
    private string _newMessageContent = string.Empty;
    private bool _isSending;
    private bool _isLoadingMessages = true;
    private bool _isInternalNoteMode;
    private bool _sidebarOpen = true;
    private string? _customerName;
    private readonly Dictionary<Guid, string> _userNameCache = new();

    // Pending attachment state
    private AttachmentDto? _pendingAttachment;
    private string? _pendingAttachmentPreviewUrl;
    private bool _isUploadingAttachment;

    // Lightbox state
    private string? _lightboxUrl;
    private string? _lightboxFileName;

    private void OpenLightbox(string url, string? fileName)
    {
        _lightboxUrl = url;
        _lightboxFileName = fileName;
        StateHasChanged();
    }

    private void CloseLightbox()
    {
        _lightboxUrl = null;
        _lightboxFileName = null;
        StateHasChanged();
    }

    protected override async Task OnInitializedAsync()
    {
        if (!RendererInfo.IsInteractive)
            return;

        HubService.OnMessageReceived += HandleMessageReceived;
        HubService.OnMessageUpdated += HandleMessageUpdated;
        HubService.OnMessageDeleted += HandleMessageDeleted;

        await LoadDetailAsync();
        _ = ConnectHubAsync();
    }

    private async Task LoadDetailAsync()
    {
        _isLoadingMessages = true;

        var result = await GatewayService.GetTicketDetailAsync(Ticket.Id);
        if (result.IsSuccess)
        {
            _messages = result.Value.Messages;
            foreach (var (key, name) in result.Value.UserNames)
                if (Guid.TryParse(key, out var id))
                    _userNameCache[id] = name;

            if (Ticket.CustomerId.HasValue && _userNameCache.TryGetValue(Ticket.CustomerId.Value, out var customerName))
                _customerName = customerName;
        }
        else
        {
            Logger.LogError("TicketDetailSupporter failed to load detail for ticket {TicketId}: {Error}", Ticket.Id, result.Error);
        }

        _isLoadingMessages = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task ConnectHubAsync()
    {
        try { await HubService.StartAsync(); }
        catch (Exception ex) { Logger.LogError(ex, "TicketDetailSupporter failed to start hub"); return; }

        var waited = 0;
        while (!HubService.IsConnected && waited < 5000) { await Task.Delay(100); waited += 100; }

        if (HubService.IsConnected)
            _ = HubService.JoinTicketGroupAsync(Ticket.Id);
    }

    private async Task HandleFileSelected(InputFileChangeEventArgs e)
    {
        Logger.LogInformation("HandleFileSelected triggered, file: {FileName}", e.File?.Name ?? "null");

        if (_isUploadingAttachment || _pendingAttachment is not null) return;

        var file = e.File;
        if (file is null) return;

        _isUploadingAttachment = true;
        await InvokeAsync(StateHasChanged);

        try
        {
            await using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024); // 10 MB limit
            var result = await MessageSdkService.UploadAttachmentAsync(stream, file.Name, file.ContentType);

            if (result.IsSuccess)
            {
                _pendingAttachment = result.Value;
                if (file.ContentType.StartsWith("image/"))
                    _pendingAttachmentPreviewUrl = _pendingAttachment.Url;
                else
                    _pendingAttachmentPreviewUrl = null;
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
            isInternalNote: _isInternalNoteMode,
            attachmentUrl: _pendingAttachment?.Url,
            attachmentFileName: _pendingAttachment?.FileName,
            attachmentContentType: _pendingAttachment?.ContentType);

        if (result.IsSuccess)
        {
            _newMessageContent = string.Empty;
            _pendingAttachment = null;
            _pendingAttachmentPreviewUrl = null;
        }
        else
        {
            Logger.LogError("TicketDetailSupporter failed to send message: {Code} - {Description}",
                result.Error.Code, result.Error.Description);
        }

        _isSending = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task UpdateStatusAsync(TicketStatus newStatus)
    {
        var result = await TicketService.UpdateTicketStatusAsync(Ticket.Id, newStatus);
        if (result.IsSuccess) { Ticket.Status = (int)newStatus; StateHasChanged(); }
        else Logger.LogError("TicketDetailSupporter failed to update status: {Error}", result.Error);
    }

    private void HandleMessageReceived(MessageDto message)
    {
        if (message.TicketId != Ticket.Id || _messages.Any(m => m.Id == message.Id)) return;
        _messages.Add(message);
        _ = ResolveUserNamesAsync(new[] { message }).ContinueWith(_ => InvokeAsync(StateHasChanged));
    }

    private void HandleMessageUpdated(MessageDto message)
    {
        var i = _messages.FindIndex(m => m.Id == message.Id);
        if (i >= 0) { _messages[i] = message; InvokeAsync(StateHasChanged); }
    }

    private void HandleMessageDeleted(int messageId)
    {
        var msg = _messages.FirstOrDefault(m => m.Id == messageId);
        if (msg is not null) { msg.IsDeleted = true; msg.Content = "[deleted]"; InvokeAsync(StateHasChanged); }
    }

    private async Task ResolveUserNamesAsync(IEnumerable<MessageDto> messages)
    {
        if (!_userNameCache.ContainsKey(UserContext.UserId))
            _userNameCache[UserContext.UserId] = UserContext.FullName;

        var unknownIds = messages
            .Where(m => m.UserId.HasValue && m.UserId.Value != Guid.Empty)
            .Select(m => m.UserId!.Value).Distinct()
            .Where(id => !_userNameCache.ContainsKey(id)).ToList();

        var tasks = unknownIds.Select(async userId =>
        {
            try { var r = await UserService.GetUserDetailsAsync(userId); return (userId, name: r.IsSuccess ? r.Value.name : (string?)null); }
            catch { return (userId, name: (string?)null); }
        });

        foreach (var (userId, name) in await Task.WhenAll(tasks))
            if (name is not null) _userNameCache[userId] = name;
    }

    private void NavigateBack() => NavigationManager.NavigateTo("/tickets");

    private bool IsOwnMessage(MessageDto m) => UserContext.UserId != Guid.Empty && m.UserId == UserContext.UserId;

    private string GetDisplayName(Guid? userId)
    {
        if (userId is null) return "Unknown";
        if (userId == UserContext.UserId) return "You";
        if (_userNameCache.TryGetValue(userId.Value, out var name)) return name;
        return "Loading...";
    }

    private string GetAssigneeName(Guid? userId)
    {
        if (userId is null) return string.Empty;
        if (userId == UserContext.UserId) return "You";
        if (_userNameCache.TryGetValue(userId.Value, out var name)) return name;
        return userId.Value.ToString()[..8] + "…";
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
        0 => "tds-tag-teal",
        1 => "tds-tag-amber",
        2 => "tds-tag-indigo",
        3 => "tds-tag-slate",
        4 => "tds-tag-red",
        _ => "tds-tag-slate"
    };

    public async ValueTask DisposeAsync()
    {
        HubService.OnMessageReceived -= HandleMessageReceived;
        HubService.OnMessageUpdated -= HandleMessageUpdated;
        HubService.OnMessageDeleted -= HandleMessageDeleted;
        await HubService.DisposeAsync();
    }
}
