using CCP.Shared.UIContext;
using CCP.Shared.ValueObjects;
using MessagingService.Sdk.Services;
using Microsoft.AspNetCore.Components;
using TicketService.Sdk.Services.Ticket;

namespace CCP.UI.Components.Dashboard;

public partial class SaasDashboardStaff : ComponentBase
{
    [Inject] private ITicketService TicketService { get; set; } = default!;
    [Inject] private IMessageSdkService MessageService { get; set; } = default!;
    [Inject] private IUIUserContext UserContext { get; set; } = default!;
    [Inject] private ILogger<SaasDashboardStaff> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private int? _allOpenCount;
    private int? _assignedCount;
    private int? _waitingForSupporterCount;
    private int? _waitingForCustomerCount;
    private List<StaffFeedEntry>? _feedEntries;

    protected override async Task OnInitializedAsync()
    {
        if (!RendererInfo.IsInteractive)
            return;

        await LoadDashboardAsync();
    }

    private async Task LoadDashboardAsync()
    {
        // Fetch all tickets (for global open count) and assigned tickets in parallel
        var allTicketsTask = TicketService.GetTickets();
        var assignedTicketsTask = TicketService.GetTickets(assignedUserId: UserContext.UserId);
        await Task.WhenAll(allTicketsTask, assignedTicketsTask);

        var allTicketsResult = await allTicketsTask;
        if (allTicketsResult.IsSuccess && allTicketsResult.Value is not null)
            _allOpenCount = allTicketsResult.Value.Count(t => t.Status != (int)TicketStatus.Closed);
        else
            _allOpenCount = 0;

        var ticketsResult = await assignedTicketsTask;

        if (ticketsResult.IsFailure || ticketsResult.Value is null)
        {
            Logger.LogError("SaasDashboardStaff failed to load assigned tickets for user {UserId}: {Error}",
                UserContext.UserId, ticketsResult.Error);
            _assignedCount = 0;
            _waitingForSupporterCount = 0;
            _waitingForCustomerCount = 0;
            _feedEntries = new();
            StateHasChanged();
            return;
        }

        var tickets = ticketsResult.Value;

        _assignedCount = tickets.Count(t => t.Status != (int)TicketStatus.Closed);
        _waitingForSupporterCount = tickets.Count(t => t.Status == (int)TicketStatus.WaitingForSupport);
        _waitingForCustomerCount = tickets.Count(t => t.Status == (int)TicketStatus.WaitingForCustomer);

        StateHasChanged();

        // Build feed: fetch the latest message for each open ticket in parallel,
        // take the 10 most recent, sorted by message time descending.
        var openTickets = tickets
            .Where(t => t.Status != (int)TicketStatus.Closed)
            .OrderByDescending(t => t.CreatedAt)
            .Take(20) // cap concurrent requests
            .ToList();

        var feedTasks = openTickets.Select(async ticket =>
        {
            try
            {
                var msgResult = await MessageService.GetMessagesByTicketIdAsync(ticket.Id, limit: 1);
                if (msgResult.IsFailure || msgResult.Value is null || !msgResult.Value.Items.Any())
                    return null;

                var latestMsg = msgResult.Value.Items.First();

                // Skip internal notes — supporters shouldn't see these surfaced in the dashboard
                if (latestMsg.IsInternalNote)
                    return null;

                var isOwnMessage = latestMsg.UserId == UserContext.UserId;
                var senderName = isOwnMessage ? "You" : "A customer";
                var actionText = "sent a message on ticket";

                return new StaffFeedEntry
                {
                    TicketId = ticket.Id,
                    TicketTitle = ticket.Title ?? $"Ticket #{ticket.Id}",
                    TicketStatus = ticket.Status,
                    SenderName = senderName,
                    ActionText = actionText,
                    OccurredAt = latestMsg.CreatedAtUtc ?? DateTimeOffset.UtcNow
                };
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to fetch latest message for ticket {TicketId}", ticket.Id);
                return null;
            }
        });

        var feedResults = await Task.WhenAll(feedTasks);

        _feedEntries = feedResults
            .Where(e => e is not null)
            .Cast<StaffFeedEntry>()
            .OrderByDescending(e => e.OccurredAt)
            .Take(10)
            .ToList();

        StateHasChanged();
    }

    private void NavigateToTicket(int ticketId) =>
        Navigation.NavigateTo($"/inbox?ticketId={ticketId}");

    private static string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "?";
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2) return $"{parts[0][0]}{parts[1][0]}".ToUpper();
        return name.Length > 0 ? name[0].ToString().ToUpper() : "?";
    }

    private static string GetStatusLabel(int status) => status switch
    {
        (int)TicketStatus.Open => "Open",
        (int)TicketStatus.WaitingForCustomer => "Waiting for Customer",
        (int)TicketStatus.WaitingForSupport => "Waiting for Support",
        (int)TicketStatus.Closed => "Closed",
        (int)TicketStatus.Blocked => "Blocked",
        _ => "Unknown"
    };

    private static string GetStatusTagClass(int status) => status switch
    {
        (int)TicketStatus.Open => "tag-teal",
        (int)TicketStatus.WaitingForCustomer => "tag-amber",
        (int)TicketStatus.WaitingForSupport => "tag-indigo",
        (int)TicketStatus.Closed => "tag-slate",
        (int)TicketStatus.Blocked => "tag-red",
        _ => "tag-slate"
    };

    private static string GetDotClass(int status) => status switch
    {
        (int)TicketStatus.WaitingForSupport => "dot-red",
        (int)TicketStatus.WaitingForCustomer => "dot-amber",
        (int)TicketStatus.Open => "dot-green",
        _ => "dot-slate"
    };

    private static string GetRelativeTime(DateTimeOffset occurredAt)
    {
        var diff = DateTimeOffset.UtcNow - occurredAt;
        if (diff.TotalMinutes < 1) return "just now";
        if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} min ago";
        if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} hrs ago";
        if (diff.TotalDays < 2) return "Yesterday";
        return $"{(int)diff.TotalDays} days ago";
    }

    private sealed class StaffFeedEntry
    {
        public int TicketId { get; init; }
        public string TicketTitle { get; init; } = string.Empty;
        public int TicketStatus { get; init; }
        public string SenderName { get; init; } = string.Empty;
        public string ActionText { get; init; } = string.Empty;
        public DateTimeOffset OccurredAt { get; init; }
    }
}
