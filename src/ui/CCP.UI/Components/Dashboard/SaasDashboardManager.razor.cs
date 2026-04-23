using Gateway.Sdk.Services;
using CCP.Shared.UIContext;
using CCP.Shared.ValueObjects;
using Microsoft.AspNetCore.Components;
using TicketService.Sdk.Dtos;
using TicketService.Sdk.Services.Ticket;

namespace CCP.UI.Components.Dashboard;

public partial class SaasDashboardManager : ComponentBase
{
    [Inject] private ITicketService TicketService { get; set; } = default!;
    [Inject] private IGatewayService GatewayService { get; set; } = default!;
    [Inject] private IUIUserContext UserContext { get; set; } = default!;
    [Inject] private ILogger<SaasDashboardManager> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private ManagerStatsSdkDto? _stats;
    private List<TicketHistoryEntryDto>? _feedEntries;
    private Dictionary<string, string> _supporterNames = new();

    protected override async Task OnInitializedAsync()
    {
        if (!RendererInfo.IsInteractive)
            return;
        await Task.WhenAll(LoadDashboardAsync(), LoadFeedAsync());
    }

    private async Task LoadDashboardAsync()
    {
        var result = await GatewayService.GetManagerDashboardAsync();
        if (result.IsSuccess)
        {
            _stats = result.Value.Stats;
            _supporterNames = result.Value.UserNames;
        }
        else
        {
            Logger.LogError("Failed to load manager dashboard: {Code} - {Description}",
                result.Error.Code, result.Error.Description);
        }
        await InvokeAsync(StateHasChanged);
    }

    private async Task LoadFeedAsync()
    {
        var result = await TicketService.GetOrgHistoryAsync(limit: 10);
        if (result.IsSuccess && result.Value is not null)
            _feedEntries = result.Value;
        else
            Logger.LogError("SaasDashboardManager failed to load org history: {Error}", result.Error);

        await InvokeAsync(StateHasChanged);
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

    private string GetSupporterName(Guid userId)
    {
        var key = userId.ToString();
        if (_supporterNames.TryGetValue(key, out var name)) return name;
        return userId.ToString()[..8] + "…";
    }

    private static string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "?";
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2) return $"{parts[0][0]}{parts[1][0]}".ToUpper();
        return name.Length > 0 ? name[0].ToString().ToUpper() : "?";
    }
}
