using CCP.Shared.UIContext;
using CCP.Shared.ValueObjects;
using IdentityService.Sdk.Services.User;
using Microsoft.AspNetCore.Components;
using TicketService.Sdk.Dtos;
using TicketService.Sdk.Services.Ticket;

namespace CCP.UI.Components.MyTicketsComponents;

public partial class MyTicketsInternalUser : ComponentBase
{
    [Inject] private ITicketService TicketService { get; set; } = default!;
    [Inject] private IUserService UserService { get; set; } = default!;
    [Inject] private IUIUserContext UserContext { get; set; } = default!;
    [Inject] private ILogger<MyTicketsInternalUser> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private List<TicketSdkDto>? _tickets;
    private List<TicketSdkDto> _filtered = new();
    private string _activeFilter = "All";
    private string _searchQuery = string.Empty;
    private bool _isLoading = true;
    private string? _errorMessage;
    private readonly Dictionary<Guid, string> _customerNameCache = new();

    private const int PageSize = 10;
    private int _currentPage = 1;
    private int TotalPages => (int)Math.Ceiling(_filtered.Count / (double)PageSize);
    private IEnumerable<TicketSdkDto> PagedItems => _filtered.Skip((_currentPage - 1) * PageSize).Take(PageSize);

    private readonly List<(string Key, string Label)> _filters = new()
    {
        ("All",             "All"),
        ("Open",            "Open"),
        ("WaitingCustomer", "Waiting for Customer"),
        ("WaitingSupport",  "Waiting for Support"),
        ("Blocked",         "Blocked"),
        ("Closed",          "Closed"),
    };

    protected override async Task OnInitializedAsync()
    {
        if (!RendererInfo.IsInteractive)
            return;

        await LoadTicketsAsync();
    }

    private async Task LoadTicketsAsync()
    {
        _isLoading = true;

        var result = await TicketService.GetTickets(assignedUserId: UserContext.UserId);
        if (result.IsSuccess && result.Value is not null)
        {
            _tickets = result.Value
                .OrderByDescending(t => t.CreatedAt)
                .ToList();
            ApplyFilter();
            await ResolveCustomerNamesAsync();
        }
        else
        {
            Logger.LogError("MyTicketsInternalUser failed to load tickets for user {UserId}: {Error}",
                UserContext.UserId, result.Error);
            _errorMessage = "We couldn't load your assigned tickets right now. Please try refreshing the page.";
        }

        _isLoading = false;
        StateHasChanged();
    }

    private async Task ResolveCustomerNamesAsync()
    {
        if (_tickets is null) return;

        var unknownIds = _tickets
            .Where(t => t.CustomerId.HasValue && !_customerNameCache.ContainsKey(t.CustomerId.Value))
            .Select(t => t.CustomerId!.Value)
            .Distinct()
            .ToList();

        if (!unknownIds.Any()) return;

        var tasks = unknownIds.Select(async customerId =>
        {
            var nameResult = await UserService.GetUserDetailsAsync(customerId);
            return (customerId, name: nameResult.IsSuccess
                ? nameResult.Value.name
                : customerId.ToString()[..8] + "…");
        });

        var results = await Task.WhenAll(tasks);
        foreach (var (customerId, name) in results)
            _customerNameCache[customerId] = name;

        StateHasChanged();
    }

    private string GetCustomerName(Guid? customerId)
    {
        if (customerId is null) return "Unknown";
        if (_customerNameCache.TryGetValue(customerId.Value, out var name)) return name;
        return customerId.Value.ToString()[..8] + "…";
    }

    private void ApplyFilter()
    {
        if (_tickets is null) return;
        var query = _tickets.AsEnumerable();

        query = _activeFilter switch
        {
            "Open"            => query.Where(t => t.Status == (int)TicketStatus.Open),
            "WaitingCustomer" => query.Where(t => t.Status == (int)TicketStatus.WaitingForCustomer),
            "WaitingSupport"  => query.Where(t => t.Status == (int)TicketStatus.WaitingForSupport),
            "Blocked"         => query.Where(t => t.Status == (int)TicketStatus.Blocked),
            "Closed"          => query.Where(t => t.Status == (int)TicketStatus.Closed),
            _                 => query
        };

        if (!string.IsNullOrWhiteSpace(_searchQuery))
            query = query.Where(t => t.Title != null &&
                t.Title.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase));

        _filtered = query.ToList();
    }

    internal (string Label, string CssKey) GetStatusInfo(int status) => status switch
    {
        (int)TicketStatus.Open => ("Open", "open"),
        (int)TicketStatus.WaitingForCustomer => ("Waiting for Customer", "reply"),
        (int)TicketStatus.WaitingForSupport => ("Waiting for Support", "support"),
        (int)TicketStatus.Blocked => ("Blocked", "blocked"),
        (int)TicketStatus.Closed => ("Closed", "closed"),
        _ => ("Unknown", "closed")
    };

    private void SetFilter(string key)
    {
        _activeFilter = key;
        _currentPage = 1;
        ApplyFilter();
        StateHasChanged();
    }

    private void SetSearch(ChangeEventArgs e)
    {
        _searchQuery = e.Value?.ToString() ?? string.Empty;
        _currentPage = 1;
        ApplyFilter();
        StateHasChanged();
    }

    private void SetPage(int page)
    {
        _currentPage = Math.Clamp(page, 1, TotalPages);
        StateHasChanged();
    }

    private int GetFilterCount(string key)
    {
        if (_tickets is null) return 0;
        return key switch
        {
            "All"            => _tickets.Count,
            "Open"           => _tickets.Count(t => t.Status == (int)TicketStatus.Open),
            "WaitingCustomer"=> _tickets.Count(t => t.Status == (int)TicketStatus.WaitingForCustomer),
            "WaitingSupport" => _tickets.Count(t => t.Status == (int)TicketStatus.WaitingForSupport),
            "Blocked"        => _tickets.Count(t => t.Status == (int)TicketStatus.Blocked),
            "Closed"         => _tickets.Count(t => t.Status == (int)TicketStatus.Closed),
            _                => 0
        };
    }

    private void NavigateToTicket(int ticketId) =>
        Navigation.NavigateTo($"/tickets/{ticketId}");

    private static string GetRelativeDate(DateTimeOffset? createdAt)
    {
        if (createdAt is null) return "Unknown date";
        var diff = DateTimeOffset.UtcNow - createdAt.Value;
        if (diff.TotalDays < 1) return "Today";
        if (diff.TotalDays < 2) return "Yesterday";
        if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} days ago";
        if (diff.TotalDays < 30) return $"{(int)(diff.TotalDays / 7)} weeks ago";
        return createdAt.Value.ToLocalTime().ToString("MMM d, yyyy");
    }
}
