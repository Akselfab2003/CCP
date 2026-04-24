using CCP.Shared.UIContext;
using CCP.Shared.ValueObjects;
using Microsoft.AspNetCore.Components;
using TicketService.Sdk.Dtos;
using TicketService.Sdk.Services.Ticket;

namespace CCP.UI.Components.Dashboard;

public partial class SaasDashboardStaff : ComponentBase
{
    [Inject] private ITicketService TicketService { get; set; } = default!;
    [Inject] private IUIUserContext UserContext { get; set; } = default!;
    [Inject] private ILogger<SaasDashboardStaff> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private int? _allOpenCount;
    private int? _assignedCount;
    private int? _waitingForSupporterCount;
    private int? _waitingForCustomerCount;
    private List<TicketHistoryEntryDto>? _feedEntries;

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

        // Load activity feed from history API
        var historyResult = await TicketService.GetMyHistoryAsync(limit: 10);
        if (historyResult.IsSuccess && historyResult.Value is not null)
            _feedEntries = historyResult.Value;
        else
            Logger.LogError("SaasDashboardStaff failed to load org history: {Error}", historyResult.Error);

        StateHasChanged();
    }

    private void NavigateToTicket(int ticketId) =>
        Navigation.NavigateTo($"/tickets/{ticketId}");

    private static string GetHistoryInitials(TicketHistoryEntryDto entry) =>
        entry.EventType switch
        {
            "MessageSent" => "💬",
            "StatusChanged" => "📋",
            "AssignedToSupporter" => "👤",
            _ => "?"
        };

    private static string GetHistoryDescription(TicketHistoryEntryDto entry) =>
        entry.EventType switch
        {
            "MessageSent" => $"New message on ticket #{entry.TicketId}" +
                (entry.NewValue is not null ? $" — {entry.NewValue}" : ""),
            "StatusChanged" => $"Ticket #{entry.TicketId} status changed to {entry.NewValue}",
            "AssignedToSupporter" => $"A supporter was assigned to ticket #{entry.TicketId}",
            _ => $"Activity on ticket #{entry.TicketId}"
        };

    private static string GetHistoryTagClass(TicketHistoryEntryDto entry) =>
        entry.EventType switch
        {
            "MessageSent" => "tag-indigo",
            "StatusChanged" when entry.NewValue == "Closed" => "tag-slate",
            "StatusChanged" when entry.NewValue == "WaitingForCustomer" => "tag-red",
            "StatusChanged" => "tag-teal",
            "AssignedToSupporter" => "tag-teal",
            _ => "tag-slate"
        };

    private static string GetHistoryTagLabel(TicketHistoryEntryDto entry) =>
        entry.EventType switch
        {
            "MessageSent" => "New Message",
            "StatusChanged" when entry.NewValue == "WaitingForCustomer" => "Waiting for Customer",
            "StatusChanged" when entry.NewValue == "WaitingForSupport" => "Waiting for Support",
            "StatusChanged" when entry.NewValue == "Closed" => "Closed",
            "StatusChanged" => entry.NewValue ?? "Status Changed",
            "AssignedToSupporter" => "Agent Assigned",
            _ => "Activity"
        };

    private static string GetHistoryDotClass(TicketHistoryEntryDto entry) =>
        entry.EventType switch
        {
            "MessageSent" => "dot-green",
            "StatusChanged" when entry.NewValue == "WaitingForCustomer" => "dot-red",
            "StatusChanged" when entry.NewValue == "Closed" => "dot-slate",
            _ => "dot-amber"
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
}
