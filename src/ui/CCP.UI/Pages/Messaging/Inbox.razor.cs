using CCP.Shared.UIContext;
using CCP.UI.Services;
using MessagingService.Sdk.Dtos;
using MessagingService.Sdk.Services;
using Microsoft.AspNetCore.Components;
using TicketService.Sdk.Dtos;
using TicketService.Sdk.Services.TicketSdk;

namespace CCP.UI.Pages.Messaging;

public partial class Inbox : ComponentBase, IAsyncDisposable
{
    [Inject] private ChatHubService HubService { get; set; } = default!;
    [Inject] private IMessageSdkService MessageSdkService { get; set; } = default!;
    [Inject] private IUIUserContext UserContext { get; set; } = default!;
    [Inject] private ITicketSdkService TicketSdkService { get; set; } = default!;
    [Inject] private ILogger<Inbox> Logger { get; set; } = default!;
    [Inject] private IdentityService.Sdk.Services.User.IUserService UserService { get; set; } = default!;

    private List<TicketSdkDto> _tickets = new();
    private List<MessageDto> _messages = new();
    private int? _activeTicketId;
    private string _newMessageContent = string.Empty;
    private bool _isSending;
    private bool _isLoadingTickets = true;
    private bool _isLoadingMessages = false;
    private bool _isInternalNoteMode = false;
    private readonly Dictionary<Guid, string> _userNameCache = new();
    private readonly HashSet<int> _recentlyAssignedTicketIds = new();

    private TicketSdkDto? ActiveTicket => _tickets.FirstOrDefault(t => t.Id == _activeTicketId);

    private int? ActiveTicketId
    {
        get => _activeTicketId;
        set
        {
            if (_activeTicketId != value)
            {
                _activeTicketId = value;
                _ = OnTicketSelectedAsync(value);
            }
        }
    }

    protected override async Task OnInitializedAsync()
    {
        if (!RendererInfo.IsInteractive)
            return;

        // Subscribe to real-time events
        HubService.OnMessageReceived += HandleMessageReceived;
        HubService.OnMessageUpdated += HandleMessageUpdated;
        HubService.OnMessageDeleted += HandleMessageDeleted;
        HubService.OnTicketAssigned += HandleTicketAssigned;

        // Start the SignalR connection independently — a hub failure must not prevent tickets from loading
        _ = HubService.StartAsync().ContinueWith(t =>
        {
            if (t.IsFaulted)
                Logger.LogError(t.Exception, "Failed to start hub connection");
        }, TaskScheduler.Default);

        // Load tickets for the current user
        await LoadTicketsAsync();
    }

    private async Task LoadTicketsAsync()
    {
        _isLoadingTickets = true;

        var userId = UserContext.UserId;
        var allTickets = new List<TicketSdkDto>();

        // Get tickets assigned to the user (supporter role)
        var assignedResult = await TicketSdkService.GetTicketsAsync(assignedUserId: userId);
        if (assignedResult.IsSuccess && assignedResult.Value is not null)
        {
            allTickets.AddRange(assignedResult.Value);
        }

        // Get tickets created by the user (customer role)
        var customerResult = await TicketSdkService.GetTicketsAsync(customerId: userId);
        if (customerResult.IsSuccess && customerResult.Value is not null)
        {
            // Only add tickets we don't already have (avoid duplicates)
            foreach (var ticket in customerResult.Value)
            {
                if (!allTickets.Any(t => t.Id == ticket.Id))
                {
                    allTickets.Add(ticket);
                }
            }
        }

        if (allTickets.Count == 0 && assignedResult.IsFailure)
        {
            Logger.LogError("Failed to load assigned tickets: {Error}", assignedResult.Error);
        }

        if (allTickets.Count == 0 && customerResult.IsFailure)
        {
            Logger.LogError("Failed to load customer tickets: {Error}", customerResult.Error);
        }

        _tickets = allTickets.OrderByDescending(t => t.CreatedAt).ToList();
        _isLoadingTickets = false;

        // Wait for hub connection before joining groups
        // Poll briefly — hub start is fire-and-forget so we can't await it directly
        var waited = 0;
        while (!HubService.IsConnected && waited < 5000)
        {
            await Task.Delay(100);
            waited += 100;
        }

        if (HubService.IsConnected)
        {
            Logger.LogInformation("Hub connected: {IsConnected} after {Waited}ms", HubService.IsConnected, waited);

            foreach (var ticket in _tickets)
            {
                Logger.LogInformation("Joining group for ticket {TicketId}", ticket.Id);
                _ = HubService.JoinTicketGroupAsync(ticket.Id).ContinueWith(t =>
                {
                    if (t.IsFaulted)
                        Logger.LogWarning("Failed to join group for ticket {TicketId}: {Error}", ticket.Id, t.Exception?.Message);
                    else
                        Logger.LogInformation("Successfully joined group for ticket {TicketId}", ticket.Id);
                }, TaskScheduler.Default);
            }
        }
        else
        {
            Logger.LogWarning("Hub not connected after waiting — ticket group subscriptions skipped");
        }
    }

    private async Task OnTicketSelectedAsync(int? ticketId)
    {
        _messages.Clear();
        _isLoadingMessages = true;
        await InvokeAsync(StateHasChanged);

        if (ticketId is null)
        {
            _isLoadingMessages = false;
            return;
        }

        await HubService.JoinTicketAsync(ticketId.Value);

        var result = await MessageSdkService.GetMessagesByTicketIdAsync(ticketId.Value);

        if (result.IsSuccess && result.Value is not null)
        {
            var messages = result.Value.Items.ToList();

            if (!UserContext.IsInternalUser)
                messages = messages.Where(m => !m.IsInternalNote).ToList();

            _messages = messages;
            await ResolveUserNamesAsync(_messages);
        }

        _isLoadingMessages = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(_newMessageContent) || _activeTicketId is null || _isSending)
            return;

        _isSending = true;

        var orgId = ActiveTicket?.OrganizationId ?? Guid.Empty;

        Logger.LogInformation("Sending message - isInternalNoteMode: {NoteMode}, passing isInternalNote: {IsNote}",
            _isInternalNoteMode, _isInternalNoteMode);

        var result = await MessageSdkService.CreateMessageAsync(
            ticketId: _activeTicketId.Value,
            organizationId: orgId,
            userId: UserContext.UserId,
            content: _newMessageContent,
            isInternalNote: _isInternalNoteMode);

        if (result.IsSuccess)
        {
            _newMessageContent = string.Empty;
        }
        else
        {
            Logger.LogError("Failed to send message: {Code} - {Description}",
                result.Error.Code, result.Error.Description);
        }

        _isSending = false;
        await InvokeAsync(StateHasChanged);
    }

    private void HandleMessageReceived(MessageDto message)
    {
        if (message.TicketId == _activeTicketId && !_messages.Any(m => m.Id == message.Id))
        {
            // Don't show internal notes to non-internal users
            if (message.IsInternalNote && !UserContext.IsInternalUser)
                return;

            _messages.Add(message);
            _ = ResolveUserNamesAsync(new[] { message }).ContinueWith(_ =>
                InvokeAsync(StateHasChanged));
        }
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

    private void HandleTicketAssigned(int ticketId, Guid assignedUserId)
    {
        Logger.LogInformation("HandleTicketAssigned fired for ticket {TicketId}", ticketId);
        var ticket = _tickets.FirstOrDefault(t => t.Id == ticketId);
        if (ticket is not null)
        {
            ticket.AssignedUserId = assignedUserId;
            _recentlyAssignedTicketIds.Add(ticketId);
            InvokeAsync(StateHasChanged);

            // Remove the pulse highlight after 4 seconds
            _ = Task.Delay(4000).ContinueWith(_ =>
            {
                _recentlyAssignedTicketIds.Remove(ticketId);
                InvokeAsync(StateHasChanged);
            }, TaskScheduler.Default);
        }
        else
        {
            Logger.LogWarning("HandleTicketAssigned: ticket {TicketId} not found in _tickets", ticketId);
        }
    }

    public async ValueTask DisposeAsync()
    {
        HubService.OnMessageReceived -= HandleMessageReceived;
        HubService.OnMessageUpdated -= HandleMessageUpdated;
        HubService.OnMessageDeleted -= HandleMessageDeleted;
        HubService.OnTicketAssigned -= HandleTicketAssigned;

        await HubService.DisposeAsync();
    }

    private bool IsOwnMessage(MessageDto message)
    {
        return UserContext.UserId != Guid.Empty && message.UserId == UserContext.UserId;
    }

    private string GetDisplayName(Guid? userId)
    {
        if (userId is null) return "Unknown";
        if (userId == UserContext.UserId) return "You";
        if (_userNameCache.TryGetValue(userId.Value, out var name)) return name;
        return "Loading...";
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

    private async Task ResolveUserNamesAsync(IEnumerable<MessageDto> messages)
    {
        // Cache the current user's name
        if (!_userNameCache.ContainsKey(UserContext.UserId))
        {
            _userNameCache[UserContext.UserId] = UserContext.FullName;
        }

        // Find unique user IDs we haven't resolved yet
        var unknownUserIds = messages
            .Where(m => m.UserId.HasValue && m.UserId.Value != Guid.Empty)
            .Select(m => m.UserId!.Value)
            .Distinct()
            .Where(id => !_userNameCache.ContainsKey(id))
            .ToList();

        // Look up each unknown user
        foreach (var userId in unknownUserIds)
        {
            try
            {
                var result = await UserService.GetUserDetailsAsync(userId);
                if (result.IsSuccess)
                {
                    _userNameCache[userId] = result.Value.name;
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Could not resolve name for user {UserId}", userId);
            }
        }
    }

    private string GetStatusLabel(int status) => status switch
    {
        0 => "Open",
        1 => "Pending",
        2 => "Resolved",
        3 => "Closed",
        _ => "Unknown"
    };

    private string GetStatusTagClass(int status) => status switch
    {
        0 => "tag-teal",
        1 => "tag-amber",
        2 => "tag-slate",
        3 => "tag-slate",
        _ => "tag-slate"
    };

    private async Task HandleKeyDown(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
        {
            await SendMessageAsync();
        }
    }


}
