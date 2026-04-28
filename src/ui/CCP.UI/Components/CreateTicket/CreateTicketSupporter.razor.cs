using CCP.Shared.UIContext;
using CCP.Shared.ValueObjects;
using IdentityService.Sdk.Models;
using IdentityService.Sdk.Services.Customer;
using Microsoft.AspNetCore.Components;
using TicketService.Sdk.Services.Ticket;

namespace CCP.UI.Components.CreateTicket;

public partial class CreateTicketSupporter : ComponentBase
{
    [Inject] private ITicketService TicketService { get; set; } = default!;
    [Inject] private ICustomerService CustomerService { get; set; } = default!;
    [Inject] private IUIUserContext UserContext { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ILogger<CreateTicketSupporter> Logger { get; set; } = default!;

    private string _title = string.Empty;
    private string _description = string.Empty;
    private bool _titleTouched;
    private bool _customerTouched;
    private bool _assignToSelf;
    private bool _isSubmitting;
    private string _customerSearch = string.Empty;
    private List<TenantMember> _allCustomers = new();
    private List<TenantMember> _searchResults = new();
    private TenantMember? _selectedCustomer;
    private string? _successMessage;
    private string? _errorMessage;

    protected override async Task OnInitializedAsync()
    {
        if (!RendererInfo.IsInteractive)
            return;

        var customersResult = await CustomerService.GetAllCustomers();
        if (customersResult.IsSuccess && customersResult.Value is not null)
            _allCustomers = customersResult.Value;
    }

    private void OnCustomerSearchInput(ChangeEventArgs e)
    {
        _customerSearch = e.Value?.ToString() ?? string.Empty;
        _searchResults = string.IsNullOrWhiteSpace(_customerSearch) || _customerSearch.Length < 2
            ? new()
            : _allCustomers
                .Where(u => $"{u.FirstName} {u.LastName}".Contains(_customerSearch, StringComparison.OrdinalIgnoreCase)
                         || u.Email.Contains(_customerSearch, StringComparison.OrdinalIgnoreCase))
                .ToList();
        StateHasChanged();
    }

    private void SelectCustomer(TenantMember user)
    {
        _selectedCustomer = user;
        _customerSearch = string.Empty;
        _searchResults.Clear();
        _customerTouched = true;
    }

    private void ClearCustomer()
    {
        _selectedCustomer = null;
        _customerSearch = string.Empty;
        _searchResults.Clear();
    }

    private async Task HandleSubmitAsync()
    {
        _titleTouched = true;
        _customerTouched = true;
        _errorMessage = null;

        if (string.IsNullOrWhiteSpace(_title) || _selectedCustomer is null)
            return;

        _isSubmitting = true;

        var result = await TicketService.CreateTicket(new TicketService.Sdk.Dtos.CreateTicketRequestDto()
        {
            Title = _title.Trim(),
            CustomerId = _selectedCustomer.Id,
            AssignedUserId = _assignToSelf ? UserContext.UserId : null,
            Description = string.IsNullOrWhiteSpace(_description) ? null : _description.Trim()
        }, origin: TicketOrigin.Manual);

        if (result.IsSuccess)
        {
            _successMessage = "Ticket created! Redirecting to the ticket overview...";
            StateHasChanged();
            await Task.Delay(1200);
            Navigation.NavigateTo("/tickets");
        }
        else
        {
            Logger.LogError("Failed to create ticket: {Code} - {Description}",
                result.Error.Code, result.Error.Description);
            _errorMessage = "Something went wrong creating the ticket. Please try again.";
            _isSubmitting = false;
        }
    }

    private static string GetInitials(TenantMember? user)
    {
        if (user is null) return "?";
        var f = user.FirstName.Length > 0 ? user.FirstName[0].ToString().ToUpper() : "";
        var l = user.LastName.Length > 0 ? user.LastName[0].ToString().ToUpper() : "";
        return string.IsNullOrEmpty(f + l) ? "?" : f + l;
    }
}
