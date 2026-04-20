using CCP.Shared.UIContext;
using CCP.Shared.ValueObjects;
using IdentityService.Sdk.Services.User;
using MessagingService.Sdk.Services;
using Microsoft.AspNetCore.Components;
using TicketService.Sdk.Dtos;
using TicketService.Sdk.Services.Ticket;

namespace CCP.UI.Components.Dashboard;

public partial class SaasDashboardManager : ComponentBase
{
    [Inject] private ITicketService TicketService { get; set; } = default!;
    [Inject] private IMessageSdkService MessageService { get; set; } = default!;
    [Inject] private IUserService UserService { get; set; } = default!;
    [Inject] private IUIUserContext UserContext { get; set; } = default!;
    [Inject] private ILogger<SaasDashboardManager> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private ManagerStatsSdkDto? _stats;
    private List<ManagerFeedEntry>? _feedEntries;
    private Dictionary<Guid, string> _supporterNames = new();

    protected override async Task OnInitializedAsync()
    {
        if (!RendererInfo.IsInteractive)
            return;
        await LoadStatsAsync();
    }

    private async Task LoadStatsAsync()
    {
        var result = await TicketService.GetManagerStatsAsync();
        if (result.IsSuccess && result.Value is not null)
            _stats = result.Value;
        else
            Logger.LogError("Failed to load manager stats: {Code} - {Description}",
                result.Error.Code, result.Error.Description);
        StateHasChanged();

        var feedTask = LoadFeedAsync();

        if (_stats?.TeamPerformance?.Any() == true)
        {
            var nameTasks = _stats.TeamPerformance.Select(async p =>
            {
                try
                {
                    var result = await UserService.GetUserDetailsAsync(p.UserId);
                    return (p.UserId, name: result.IsSuccess ? result.Value.name : p.UserId.ToString()[..8] + "…");
                }
                catch
                {
                    return (p.UserId, name: p.UserId.ToString()[..8] + "…");
                }
            });
            var nameResults = await Task.WhenAll(nameTasks);
            _supporterNames = nameResults.ToDictionary(r => r.UserId, r => r.name);
            StateHasChanged();
        }

        await feedTask;
    }

    private async Task LoadFeedAsync()
    {
        var ticketsResult = await TicketService.GetTickets();
        if (ticketsResult.IsFailure || ticketsResult.Value is null)
        {
            Logger.LogError("SaasDashboardManager failed to load tickets for feed: {Error}", ticketsResult.Error);
            _feedEntries = new();
            StateHasChanged();
            return;
        }

        var openTickets = ticketsResult.Value
            .Where(t => t.Status != (int)TicketStatus.Closed)
            .OrderByDescending(t => t.CreatedAt)
            .Take(20)
            .ToList();

        var feedTasks = openTickets.Select(async ticket =>
        {
            try
            {
                var msgResult = await MessageService.GetMessagesByTicketIdAsync(ticket.Id, limit: 1);
                if (msgResult.IsFailure || msgResult.Value is null || !msgResult.Value.Items.Any())
                    return null;

                var latestMsg = msgResult.Value.Items.First();
                if (latestMsg.IsInternalNote)
                    return null;

                return new ManagerFeedEntry
                {
                    TicketId = ticket.Id,
                    TicketTitle = ticket.Title ?? $"Ticket #{ticket.Id}",
                    TicketStatus = ticket.Status,
                    SenderUserId = latestMsg.UserId,
                    SenderName = latestMsg.UserId == UserContext.UserId ? "You" : string.Empty,
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

        var entries = feedResults
            .Where(e => e is not null)
            .Cast<ManagerFeedEntry>()
            .OrderByDescending(e => e.OccurredAt)
            .Take(10)
            .ToList();

        // Resolve names for senders we don't already know
        var unknownIds = entries
            .Where(e => e.SenderUserId.HasValue
                     && e.SenderUserId.Value != UserContext.UserId
                     && string.IsNullOrEmpty(e.SenderName))
            .Select(e => e.SenderUserId!.Value)
            .Distinct()
            .ToList();

        var nameTasks = unknownIds.Select(async userId =>
        {
            try
            {
                var result = await UserService.GetUserDetailsAsync(userId);
                return (userId, name: result.IsSuccess ? result.Value.name : "Unknown");
            }
            catch
            {
                return (userId, name: "Unknown");
            }
        });

        var nameResults = await Task.WhenAll(nameTasks);
        var nameMap = nameResults.ToDictionary(r => r.userId, r => r.name);

        _feedEntries = entries.Select(e =>
        {
            if (string.IsNullOrEmpty(e.SenderName) && e.SenderUserId.HasValue
                && nameMap.TryGetValue(e.SenderUserId.Value, out var resolvedName))
            {
                return e with { SenderName = resolvedName };
            }
            return e;
        }).ToList();

        StateHasChanged();
    }

    private void NavigateToTicket(int ticketId) =>
        Navigation.NavigateTo($"/tickets/{ticketId}");

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

    private static string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "?";
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2) return $"{parts[0][0]}{parts[1][0]}".ToUpper();
        return name.Length > 0 ? name[0].ToString().ToUpper() : "?";
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

    private sealed record ManagerFeedEntry
    {
        public int TicketId { get; init; }
        public string TicketTitle { get; init; } = string.Empty;
        public int TicketStatus { get; init; }
        public Guid? SenderUserId { get; init; }
        public string SenderName { get; init; } = string.Empty;
        public DateTimeOffset OccurredAt { get; init; }
    }
}
