using CCP.Shared.UIContext;
using CCP.Shared.ValueObjects;
using IdentityService.Sdk.Models;
using IdentityService.Sdk.Services.Supporter;
using IdentityService.Sdk.Services.User;
using Microsoft.AspNetCore.Components;
using TicketService.Sdk.Dtos;
using TicketService.Sdk.Services.Assignment;
using TicketService.Sdk.Services.Ticket;

namespace CCP.UI.Pages.Tickets;

public partial class TicketOverview : ComponentBase
{
    [Inject] private ITicketService TicketService { get; set; } = default!;
    [Inject] private IAssignmentService AssignmentService { get; set; } = default!;
    [Inject] private IUserService UserService { get; set; } = default!;
    [Inject] private ISupporterService SupporterService { get; set; } = default!;
    [Inject] private IUIUserContext UserContext { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private ILogger<TicketOverview> Logger { get; set; } = default!;

    // Tickets
    private List<TicketSdkDto> _tickets = new();
    private List<TicketSdkDto> _filteredTickets = new();
    private string _statusFilter = "Open";
    private bool _isLoading = true;
    private string? _errorMessage;
    private bool _isAssigning;

    // User name cache: AssignedUserId → display name
    private Dictionary<Guid, string> _userNames = new();
    private bool _isLoadingNames;

    // Pagination
    private const int PageSize = 25;
    private int _currentPage = 1;
    private int TotalPages => (int)Math.Ceiling(_filteredTickets.Count / (double)PageSize);
    private IEnumerable<TicketSdkDto> PagedTickets =>
        _filteredTickets.Skip((_currentPage - 1) * PageSize).Take(PageSize);

    // Side panel
    private TicketSdkDto? _selectedTicket;
    private bool _isPanelOpen;
    private bool _isUpdatingAssignment;

    // Supporter search state (manager-only feature)
    private string _supporterSearchTerm = string.Empty;
    private List<TenantMember> _allSupporters = new();
    private List<TenantMember> _supporterSearchResults = new();
    private string? _searchError;

    private bool IsManager => UserContext.Role == UserRole.Manager || UserContext.Role == UserRole.Admin;

    private bool CanGoToTicket(TicketSdkDto ticket) =>
        IsManager || ticket.AssignedUserId == UserContext.UserId;

    protected override async Task OnInitializedAsync()
    {
        if (!RendererInfo.IsInteractive)
            return;

        if (!UserContext.IsInternalUser)
        {
            NavigationManager.NavigateTo("/");
            return;
        }

        var supportersResult = await SupporterService.GetAllSupporters();
        if (supportersResult.IsSuccess && supportersResult.Value is not null)
            _allSupporters = supportersResult.Value;

        await LoadTicketsAsync();
    }

    private async Task LoadTicketsAsync()
    {
        _isLoading = true;
        _errorMessage = null;

        var result = await TicketService.GetTickets();

        if (result.IsSuccess && result.Value is not null)
        {
            _tickets = result.Value.OrderByDescending(t => t.CreatedAt).ToList();
        }
        else
        {
            Logger.LogError(
                "TicketOverview failed to load tickets for user {UserId} (role: {Role}). " +
                "Code: {ErrorCode} | Description: {ErrorDescription}",
                UserContext.UserId,
                UserContext.Role,
                result.Error.Code,
                result.Error.Description);
            _errorMessage = $"Failed to load tickets ({result.Error.Code}: {result.Error.Description})";
            _tickets = new();
        }

        _isLoading = false;
        ApplyFilter();

        // Resolve names for all assigned users visible after filtering
        await ResolveUserNamesAsync();
    }

    // Collects all unique AssignedUserIds across ALL tickets (not just the
    // current page) so that names are ready when the user pages forward.
    private async Task ResolveUserNamesAsync()
    {
        var unknownIds = _tickets
            .Where(t => t.AssignedUserId.HasValue && !_userNames.ContainsKey(t.AssignedUserId.Value))
            .Select(t => t.AssignedUserId!.Value)
            .Distinct()
            .ToList();

        if (!unknownIds.Any()) return;

        _isLoadingNames = true;
        StateHasChanged();

        // Fire all lookups in parallel — total wait = slowest single request
        var tasks = unknownIds.Select(async userId =>
        {
            var nameResult = await UserService.GetUserDetailsAsync(userId);
            return (userId, name: nameResult.IsSuccess
                ? nameResult.Value.name
                : userId.ToString()[..8] + "…");
        });

        var results = await Task.WhenAll(tasks);
        foreach (var (userId, name) in results)
            _userNames[userId] = name;

        _isLoadingNames = false;
        StateHasChanged();
    }

    // Returns display name for an assigned user, with loading/fallback states
    private string GetAssigneeName(Guid? userId)
    {
        if (userId is null) return string.Empty;
        if (userId == UserContext.UserId) return "You";
        if (_userNames.TryGetValue(userId.Value, out var name)) return name;
        return _isLoadingNames ? "Loading..." : userId.Value.ToString()[..8] + "…";
    }

    // Filter

    private void ApplyFilter()
    {
        _filteredTickets = _statusFilter switch
        {
            "Open" => _tickets.Where(t => t.Status == (int)TicketStatus.Open).ToList(),
            "WaitingForCustomer" => _tickets.Where(t => t.Status == (int)TicketStatus.WaitingForCustomer).ToList(),
            "WaitingForSupport" => _tickets.Where(t => t.Status == (int)TicketStatus.WaitingForSupport).ToList(),
            "Closed" => _tickets.Where(t => t.Status == (int)TicketStatus.Closed).ToList(),
            "Blocked" => _tickets.Where(t => t.Status == (int)TicketStatus.Blocked).ToList(),
            "Unassigned" => _tickets.Where(t => t.AssignedUserId is null).ToList(),
            _ => _tickets.ToList()
        };
        _currentPage = 1; // reset to page 1 whenever filter changes
        StateHasChanged();
    }

    private void SetFilter(string filter)
    {
        _statusFilter = filter;
        ApplyFilter();
    }

    // Pagination

    private void GoToPage(int page)
    {
        if (page < 1 || page > TotalPages) return;
        _currentPage = page;
        StateHasChanged();
    }

    // Assignment

    private async Task SelfAssignAsync(int ticketId)
    {
        if (_isAssigning) return;
        _isAssigning = true;
        _errorMessage = null;

        var result = await AssignmentService.AssignTicketToUserAsync(ticketId, UserContext.UserId);

        if (result.IsSuccess)
        {
            var ticket = _tickets.FirstOrDefault(t => t.Id == ticketId);
            if (ticket is not null)
                ticket.AssignedUserId = UserContext.UserId;
            ApplyFilter();
        }
        else
        {
            Logger.LogError("Failed to assign ticket {TicketId}: {Error}", ticketId, result.Error);
            _errorMessage = $"Failed to assign ticket: {result.Error.Description}";
            StateHasChanged();
        }

        _isAssigning = false;
    }

    // Side panel

    private void OpenPanel(TicketSdkDto ticket)
    {
        _selectedTicket = ticket;
        _supporterSearchTerm = string.Empty;
        _supporterSearchResults = new();
        _searchError = null;
        _isPanelOpen = true;
    }

    private void ClosePanel() => _isPanelOpen = false;

    private void HandleSupporterSearchInput(ChangeEventArgs e)
    {
        _supporterSearchTerm = e.Value?.ToString() ?? string.Empty;
        _supporterSearchResults = string.IsNullOrWhiteSpace(_supporterSearchTerm) || _supporterSearchTerm.Length < 2
            ? new()
            : _allSupporters
                .Where(u => $"{u.FirstName} {u.LastName}".Contains(_supporterSearchTerm, StringComparison.OrdinalIgnoreCase)
                         || u.Email.Contains(_supporterSearchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
        StateHasChanged();
    }

    private async Task AssignToSupporterAsync(Guid supporterUserId)
    {
        if (_selectedTicket is null || _isUpdatingAssignment) return;
        _isUpdatingAssignment = true;

        var result = await AssignmentService.AssignTicketToUserAsync(_selectedTicket.Id, supporterUserId);
        if (result.IsSuccess)
        {
            // Update both _selectedTicket and the matching entry in _tickets
            _selectedTicket.AssignedUserId = supporterUserId;
            var ticket = _tickets.FirstOrDefault(t => t.Id == _selectedTicket.Id);
            if (ticket is not null) ticket.AssignedUserId = supporterUserId;

            // Resolve name if not yet cached
            if (!_userNames.ContainsKey(supporterUserId))
            {
                var found = _supporterSearchResults.FirstOrDefault(u => u.Id == supporterUserId);
                if (found is not null)
                    _userNames[supporterUserId] = $"{found.FirstName} {found.LastName}".Trim();
            }

            _supporterSearchTerm = string.Empty;
            _supporterSearchResults = new();
            ApplyFilter();
        }
        else
        {
            _searchError = $"Failed to assign: {result.Error.Description}";
        }

        _isUpdatingAssignment = false;
        StateHasChanged();
    }

    private async Task SelfAssignFromPanelAsync()
    {
        if (_selectedTicket is null) return;
        await SelfAssignAsync(_selectedTicket.Id);
        // Sync selected ticket with the updated _tickets list
        _selectedTicket = _tickets.FirstOrDefault(t => t.Id == _selectedTicket.Id)
                          ?? _selectedTicket;
        StateHasChanged();
    }

    private void NavigateToTicket(int ticketId) =>
        NavigationManager.NavigateTo($"/tickets/{ticketId}");

    // Helpers

    private string GetStatusLabel(int status) => status switch
    {
        (int)TicketStatus.Open => "Open",
        (int)TicketStatus.WaitingForCustomer => "Waiting for Customer",
        (int)TicketStatus.WaitingForSupport => "Waiting for Support",
        (int)TicketStatus.Closed => "Closed",
        (int)TicketStatus.Blocked => "Blocked",
        _ => "Unknown"
    };

    private string GetStatusTagClass(int status) => status switch
    {
        (int)TicketStatus.Open => "tag-teal",
        (int)TicketStatus.WaitingForCustomer => "tag-amber",
        (int)TicketStatus.WaitingForSupport => "tag-indigo",
        (int)TicketStatus.Closed => "tag-slate",
        (int)TicketStatus.Blocked => "tag-red",
        _ => "tag-slate"
    };

    private string GetAssigneePillClass(Guid? userId)
    {
        if (userId is null) return "to-assigned-unassigned";
        if (userId == UserContext.UserId) return "to-assigned-you";
        return "to-assigned-other";
    }
}
