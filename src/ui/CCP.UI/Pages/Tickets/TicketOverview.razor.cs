using CCP.Shared.UIContext;
using CCP.Shared.ValueObjects;
using IdentityService.Sdk.Services.User;
using Microsoft.AspNetCore.Components;
using TicketService.Sdk.Dtos;
using TicketService.Sdk.Services.TicketSdk;

namespace CCP.UI.Pages.Tickets;

public partial class TicketOverview : ComponentBase
{
    [Inject] private ITicketSdkService TicketSdkService { get; set; } = default!;
    [Inject] private IUserService UserService { get; set; } = default!;
    [Inject] private IUIUserContext UserContext { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private ILogger<TicketOverview> Logger { get; set; } = default!;

    // ── Tickets ──────────────────────────────────────────────────────────
    private List<TicketSdkDto> _tickets = new();
    private List<TicketSdkDto> _filteredTickets = new();
    private string _statusFilter = "Open";
    private bool _isLoading = true;
    private string? _errorMessage;
    private bool _isAssigning;

    // ── User name cache: AssignedUserId → display name ───────────────────
    private Dictionary<Guid, string> _userNames = new();
    private bool _isLoadingNames;

    // ── Pagination ────────────────────────────────────────────────────────
    private const int PageSize = 25;
    private int _currentPage = 1;
    private int TotalPages => (int)Math.Ceiling(_filteredTickets.Count / (double)PageSize);
    private IEnumerable<TicketSdkDto> PagedTickets =>
        _filteredTickets.Skip((_currentPage - 1) * PageSize).Take(PageSize);

    // ── Manager assign modal ──────────────────────────────────────────────
    private bool _showAssignModal;
    private int _modalTicketId;
    private string _assignTargetUserIdInput = string.Empty;
    private string? _assignModalError;

    private bool IsManager => UserContext.Role == UserRole.Manager || UserContext.Role == UserRole.Admin;

    // ─────────────────────────────────────────────────────────────────────

    protected override async Task OnInitializedAsync()
    {
        if (!RendererInfo.IsInteractive)
            return;

        if (!UserContext.IsInternalUser)
        {
            NavigationManager.NavigateTo("/");
            return;
        }

        await LoadTicketsAsync();
    }

    private async Task LoadTicketsAsync()
    {
        _isLoading = true;
        _errorMessage = null;

        var result = await TicketSdkService.GetTicketsAsync();

        if (result.IsSuccess && result.Value is not null)
        {
            _tickets = result.Value.OrderByDescending(t => t.CreatedAt).ToList();
        }
        else
        {
            Logger.LogError("Failed to load tickets: {Error}", result.Error);
            _errorMessage = "Failed to load tickets. Please try again.";
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

    // ── Filter ────────────────────────────────────────────────────────────

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

    // ── Pagination ────────────────────────────────────────────────────────

    private void GoToPage(int page)
    {
        if (page < 1 || page > TotalPages) return;
        _currentPage = page;
        StateHasChanged();
    }

    // ── Assignment ────────────────────────────────────────────────────────

    private async Task SelfAssignAsync(int ticketId)
    {
        if (_isAssigning) return;
        _isAssigning = true;
        _errorMessage = null;

        var result = await TicketSdkService.AssignTicketAsync(ticketId, UserContext.UserId);

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

    private void OpenAssignModal(int ticketId)
    {
        _modalTicketId = ticketId;
        _assignTargetUserIdInput = string.Empty;
        _assignModalError = null;
        _showAssignModal = true;
    }

    private void CloseAssignModal() => _showAssignModal = false;

    private async Task SubmitAssignModalAsync()
    {
        if (!Guid.TryParse(_assignTargetUserIdInput, out var parsedGuid))
        {
            _assignModalError = "Invalid User ID. Please enter a valid GUID.";
            return;
        }

        _assignModalError = null;
        var result = await TicketSdkService.AssignTicketAsync(_modalTicketId, parsedGuid);

        if (result.IsSuccess)
        {
            var ticket = _tickets.FirstOrDefault(t => t.Id == _modalTicketId);
            if (ticket is not null)
                ticket.AssignedUserId = parsedGuid;

            // Resolve the new assignee's name if we don't have it yet
            if (!_userNames.ContainsKey(parsedGuid))
            {
                var nameResult = await UserService.GetUserDetailsAsync(parsedGuid);
                _userNames[parsedGuid] = nameResult.IsSuccess
                    ? nameResult.Value.name
                    : parsedGuid.ToString()[..8] + "…";
            }

            CloseAssignModal();
            ApplyFilter();
        }
        else
        {
            Logger.LogError("Failed to assign ticket {TicketId}: {Error}", _modalTicketId, result.Error);
            _assignModalError = $"Failed to assign: {result.Error.Description}";
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────

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

    // CSS class for the assigned-to pill based on who the assignee is
    private string GetAssigneePillClass(Guid? userId)
    {
        if (userId is null) return "to-assigned-unassigned";
        if (userId == UserContext.UserId) return "to-assigned-you";
        return "to-assigned-other";
    }
}
