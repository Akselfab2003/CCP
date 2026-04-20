using CCP.Shared.UIContext;
using CCP.Shared.ValueObjects;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using TicketService.Sdk.Dtos;
using TicketService.Sdk.Services.Ticket;

namespace CCP.UI.Components.Dashboard;

public partial class SaasDashboardCustomer : ComponentBase
{
    [Inject] private ITicketService TicketService { get; set; } = default!;
    [Inject] private IUIUserContext UserContext { get; set; } = default!;
    [Inject] private ILogger<SaasDashboardCustomer> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private int? _openCount;
    private int? _waitingForCustomerCount;
    private int? _waitingForSupporterCount;
    private List<TicketHistoryEntryDto>? _historyEntries;

    protected override async Task OnInitializedAsync()
    {
        if (!RendererInfo.IsInteractive)
            return;

        await LoadTicketStatsAsync();
    }

    private async Task LoadTicketStatsAsync()
    {
        // Stats
        var result = await TicketService.GetTickets(CustomerId: UserContext.UserId);
        if (result.IsFailure || result.Value is null)
        {
            Logger.LogError("Failed to load customer tickets for dashboard: {Error}", result.Error);
        }
        else
        {
            var tickets = result.Value;
            _openCount = tickets.Count(t => t.Status != (int)TicketStatus.Closed);
            _waitingForCustomerCount = tickets.Count(t => t.Status == (int)TicketStatus.WaitingForCustomer);
            _waitingForSupporterCount = tickets.Count(t => t.Status == (int)TicketStatus.WaitingForSupport);
        }

        // History separated so any failure is visible in logs
        try
        {
            var historyResult = await TicketService.GetCustomerHistoryAsync(UserContext.UserId, limit: 10);
            if (historyResult.IsSuccess && historyResult.Value is not null)
                _historyEntries = historyResult.Value;
            else
                Logger.LogError("History fetch failed: {Code} - {Description}",
                    historyResult.Error.Code, historyResult.Error.Description);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception during history fetch for user {UserId}", UserContext.UserId);
        }

        StateHasChanged();
    }

    private string GetHistoryInitials(TicketHistoryEntryDto entry) =>
        entry.EventType switch
        {
            "MessageSent" => "💬",
            "StatusChanged" => "📋",
            "AssignedToSupporter" => "👤",
            _ => "?"
        };

    private string GetHistoryDescription(TicketHistoryEntryDto entry) =>
        entry.EventType switch
        {
            "MessageSent" => $"New message on ticket #{entry.TicketId}" +
                (entry.NewValue is not null ? $" — {entry.NewValue}" : ""),
            "StatusChanged" => $"Ticket #{entry.TicketId} status changed to {entry.NewValue}",
            "AssignedToSupporter" => $"A supporter was assigned to ticket #{entry.TicketId}",
            _ => $"Activity on ticket #{entry.TicketId}"
        };

    private string GetHistoryTagClass(TicketHistoryEntryDto entry) =>
        entry.EventType switch
        {
            "MessageSent" => "tag-indigo",
            "StatusChanged" when entry.NewValue == "Closed" => "tag-slate",
            "StatusChanged" when entry.NewValue == "WaitingForCustomer" => "tag-red",
            "StatusChanged" => "tag-teal",
            "AssignedToSupporter" => "tag-teal",
            _ => "tag-slate"
        };

    private string GetHistoryTagLabel(TicketHistoryEntryDto entry) =>
        entry.EventType switch
        {
            "MessageSent" => "New Message",
            "StatusChanged" when entry.NewValue == "WaitingForCustomer" => "Awaiting Your Reply",
            "StatusChanged" when entry.NewValue == "WaitingForSupport" => "With Support",
            "StatusChanged" when entry.NewValue == "Closed" => "Closed",
            "StatusChanged" => entry.NewValue ?? "Status Changed",
            "AssignedToSupporter" => "Agent Assigned",
            _ => "Activity"
        };

    private string GetHistoryDotClass(TicketHistoryEntryDto entry) =>
        entry.EventType switch
        {
            "MessageSent" => "dot-green",
            "StatusChanged" when entry.NewValue == "WaitingForCustomer" => "dot-red",
            "StatusChanged" when entry.NewValue == "Closed" => "dot-slate",
            _ => "dot-amber"
        };

    private void NavigateToTicket(int ticketId)
    {
        Navigation.NavigateTo($"/tickets/{ticketId}");
    }

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
