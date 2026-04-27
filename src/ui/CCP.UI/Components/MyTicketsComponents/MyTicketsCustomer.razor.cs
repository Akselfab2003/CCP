using CCP.Shared.UIContext;
using CCP.Shared.ValueObjects;
using Microsoft.AspNetCore.Components;
using TicketService.Sdk.Dtos;
using TicketService.Sdk.Services.Ticket;

namespace CCP.UI.Components.MyTicketsComponents;

public partial class MyTicketsCustomer : ComponentBase
{
    [Inject] private ITicketService TicketService { get; set; } = default!;
    [Inject] private IUIUserContext UserContext { get; set; } = default!;
    [Inject] private ILogger<MyTicketsCustomer> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private List<TicketSdkDto>? _tickets;
    private List<TicketSdkDto> _filtered = new();
    private string _activeFilter = "All";
    private string _searchQuery = string.Empty;
    private bool _isLoading = true;
    private string? _errorMessage;

    private const int PageSize = 10;
    private int _currentPage = 1;
    private int TotalPages => (int)Math.Ceiling(_filtered.Count / (double)PageSize);
    private IEnumerable<TicketSdkDto> PagedItems => _filtered.Skip((_currentPage - 1) * PageSize).Take(PageSize);

    private readonly List<(string Key, string Label)> _filters = new()
    {
        ("All",     "All"),
        ("Active",  "Active"),
        ("Reply",   "Awaiting Reply"),
        ("Closed",  "Resolved"),
    };

    protected override async Task OnInitializedAsync()
    {
        if (!RendererInfo.IsInteractive)
            return;

        // Customers only redirect anyone else away
        if (UserContext.Role != UserRole.Customer)
        {
            Navigation.NavigateTo("/");
            return;
        }

        await LoadTicketsAsync();
    }

    private async Task LoadTicketsAsync()
    {
        _isLoading = true;

        var result = await TicketService.GetTickets(CustomerId: UserContext.UserId);
        if (result.IsSuccess && result.Value is not null)
        {
            _tickets = result.Value
                .OrderByDescending(t => t.CreatedAt)
                .ToList();
            ApplyFilter();
        }
        else
        {
            Logger.LogError("MyTicketsCustomer failed to load tickets for customer {UserId}: {Error}",
                UserContext.UserId, result.Error);
            _errorMessage = "We couldn't load your tickets right now. Please try refreshing the page.";
        }

        _isLoading = false;
        StateHasChanged();
    }

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

    private void ApplyFilter()
    {
        if (_tickets is null) return;

        var query = _tickets.AsEnumerable();

        if (_activeFilter != "All")
            query = query.Where(t => GetFilterKey(t) == _activeFilter);

        if (!string.IsNullOrWhiteSpace(_searchQuery))
            query = query.Where(t => t.Title != null &&
                t.Title.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase));

        _filtered = query.ToList();
    }

    // Maps a ticket to the filter bucket it belongs to
    internal string GetFilterKey(TicketSdkDto ticket) => ticket.Status switch
    {
        (int)TicketStatus.Closed => "Closed",
        (int)TicketStatus.WaitingForCustomer => "Reply",
        _ => "Active"   // Open, WaitingForSupport, Blocked all count as Active
    };

    internal (string Label, string CssKey) GetStatusInfo(int status) => status switch
    {
        (int)TicketStatus.Open => ("In Progress", "active"),
        (int)TicketStatus.WaitingForCustomer => ("Awaiting Your Reply", "reply"),
        (int)TicketStatus.WaitingForSupport => ("With Support Team", "support"),
        (int)TicketStatus.Closed => ("Resolved", "closed"),
        (int)TicketStatus.Blocked => ("In Progress", "active"),  // hide Blocked from customers
        _ => ("In Progress", "active")
    };

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
