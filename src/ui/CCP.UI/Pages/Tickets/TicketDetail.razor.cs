using CCP.Shared.UIContext;
using CCP.Shared.ValueObjects;
using CCP.UI.Services;
using IdentityService.Sdk.Services.User;
using MessagingService.Sdk.Dtos;
using MessagingService.Sdk.Services;
using Microsoft.AspNetCore.Components;
using TicketService.Sdk.Dtos;
using TicketService.Sdk.Services.Ticket;

namespace CCP.UI.Pages.Tickets;

public partial class TicketDetail : ComponentBase, IAsyncDisposable
{
    [Inject] private ChatHubService HubService { get; set; } = default!;
    [Inject] private IMessageSdkService MessageSdkService { get; set; } = default!;
    [Inject] private IUIUserContext UserContext { get; set; } = default!;
    [Inject] private ITicketService TicketService { get; set; } = default!;
    [Inject] private IUserService UserService { get; set; } = default!;
    [Inject] private ILogger<TicketDetail> Logger { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    [Parameter] public int TicketId { get; set; }

    private TicketSdkDto? _ticket;
    private List<MessageDto> _messages = new();
    private string _newMessageContent = string.Empty;
    private bool _isSending;
    private bool _isLoading = true;
    private bool _isLoadingMessages = true;
    private bool _isInternalNoteMode;
    private bool _sidebarOpen = true;
    private string? _errorMessage;
    private string? _customerName;
    private readonly Dictionary<Guid, string> _userNameCache = new();

    protected override async Task OnInitializedAsync()
    {
        if (!RendererInfo.IsInteractive)
            return;

        if (!UserContext.IsInternalUser)
        {
            NavigationManager.NavigateTo("/");
            return;
        }

        HubService.OnMessageReceived += HandleMessageReceived;
        HubService.OnMessageUpdated += HandleMessageUpdated;
        HubService.OnMessageDeleted += HandleMessageDeleted;

        await LoadTicketAsync();
        await LoadMessagesAsync();

        _ = ConnectHubAsync();
    }

    private async Task LoadTicketAsync()
    {
        _isLoading = true;

        var result = await TicketService.GetTicket(TicketId);

        if (result.IsSuccess)
        {
            _ticket = result.Value;

            // Resolve customer name in the background — don't block the ticket render
            if (_ticket.CustomerId.HasValue)
                _ = ResolveCustomerNameAsync(_ticket.CustomerId.Value);
        }
        else
        {
            Logger.LogError("TicketDetail failed to load ticket {TicketId}: {Error}", TicketId, result.Error);
            _errorMessage = "Ticket not found or you don't have access to it.";
        }

        _isLoading = false;
        StateHasChanged();
    }

    private async Task ResolveCustomerNameAsync(Guid customerId)
    {
        try
        {
            var result = await UserService.GetUserDetailsAsync(customerId);
            _customerName = result.IsSuccess ? result.Value.name : null;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Could not resolve customer name for {CustomerId}", customerId);
            _customerName = null;
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadMessagesAsync()
    {
        _isLoadingMessages = true;

        var result = await MessageSdkService.GetMessagesByTicketIdAsync(TicketId);

        if (result.IsSuccess && result.Value is not null)
        {
            _messages = result.Value.Items.ToList();
            await ResolveUserNamesAsync(_messages);
        }
        else
        {
            Logger.LogError("TicketDetail failed to load messages for ticket {TicketId}: {Error}", TicketId, result.Error);
        }

        _isLoadingMessages = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task ConnectHubAsync()
    {
        try
        {
            await HubService.StartAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "TicketDetail failed to start hub connection");
            return;
        }

        var waited = 0;
        while (!HubService.IsConnected && waited < 5000)
        {
            await Task.Delay(100);
            waited += 100;
        }

        if (HubService.IsConnected)
        {
            _ = HubService.JoinTicketGroupAsync(TicketId).ContinueWith(t =>
            {
                if (t.IsFaulted)
                    Logger.LogWarning("TicketDetail failed to join group for ticket {TicketId}: {Error}", TicketId, t.Exception?.Message);
            }, TaskScheduler.Default);
        }
        else
        {
            Logger.LogWarning("TicketDetail hub not connected after waiting — real-time updates disabled");
        }
    }

    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(_newMessageContent) || _isSending || _ticket is null)
            return;

        _isSending = true;

        var result = await MessageSdkService.CreateMessageAsync(
            ticketId: TicketId,
            organizationId: _ticket.OrganizationId,
            userId: UserContext.UserId,
            content: _newMessageContent,
            isInternalNote: _isInternalNoteMode);

        if (result.IsSuccess)
        {
            _newMessageContent = string.Empty;
        }
        else
        {
            Logger.LogError("TicketDetail failed to send message: {Code} - {Description}",
                result.Error.Code, result.Error.Description);
        }

        _isSending = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task UpdateTicketStatusAsync(TicketStatus newStatus)
    {
        if (_ticket is null) return;

        var result = await TicketService.UpdateTicketStatusAsync(TicketId, newStatus);
        if (result.IsSuccess)
        {
            _ticket.Status = (int)newStatus;
            StateHasChanged();
        }
        else
        {
            Logger.LogError("TicketDetail failed to update ticket status: {Error}", result.Error);
        }
    }

    private void HandleMessageReceived(MessageDto message)
    {
        if (message.TicketId != TicketId || _messages.Any(m => m.Id == message.Id))
            return;

        _messages.Add(message);
        _ = ResolveUserNamesAsync(new[] { message }).ContinueWith(_ =>
            InvokeAsync(StateHasChanged));
    }

    private void HandleMessageUpdated(MessageDto message)
    {
        var index = _messages.FindIndex(m => m.Id == message.Id);
        if (index >= 0)
        {
            _messages[index] = message;
            InvokeAsync(StateHasChanged);
        }
    }

    private void HandleMessageDeleted(int messageId)
    {
        var message = _messages.FirstOrDefault(m => m.Id == messageId);
        if (message is not null)
        {
            message.IsDeleted = true;
            message.Content = "[deleted]";
            InvokeAsync(StateHasChanged);
        }
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
                var result = await UserService.GetUserDetailsAsync(userId);
                return (userId, name: result.IsSuccess ? result.Value.name : (string?)null);
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Could not resolve name for user {UserId}", userId);
                return (userId, name: (string?)null);
            }
        });

        var results = await Task.WhenAll(tasks);
        foreach (var (userId, name) in results)
        {
            if (name is not null)
                _userNameCache[userId] = name;
        }
    }

    private void NavigateBack() =>
        NavigationManager.NavigateTo("/tickets");

    private bool IsOwnMessage(MessageDto message) =>
        UserContext.UserId != Guid.Empty && message.UserId == UserContext.UserId;

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
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[1][0]}".ToUpper();
            return name.Length > 0 ? name[0].ToString().ToUpper() : "?";
        }
        return "?";
    }

    private async Task HandleKeyDown(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
            await SendMessageAsync();
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
        0 => "td-tag-teal",
        1 => "td-tag-amber",
        2 => "td-tag-indigo",
        3 => "td-tag-slate",
        4 => "td-tag-red",
        _ => "td-tag-slate"
    };

    public async ValueTask DisposeAsync()
    {
        HubService.OnMessageReceived -= HandleMessageReceived;
        HubService.OnMessageUpdated -= HandleMessageUpdated;
        HubService.OnMessageDeleted -= HandleMessageDeleted;

        await HubService.DisposeAsync();
    }
}
