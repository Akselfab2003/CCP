using CCP.Shared.UIContext;
using CCP.Shared.ValueObjects;
using IdentityService.Sdk.Models;
using IdentityService.Sdk.Services.User;
using Microsoft.AspNetCore.Components;
using TicketService.Sdk.Services.Ticket;

namespace CCP.UI.Components.CreateTicket;

public partial class CreateTicketSupporter : ComponentBase
{
    [Inject] private ITicketService TicketService { get; set; } = default!;
    [Inject] private IUserService UserService { get; set; } = default!;
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
    private bool _isSearching;
    private List<UserAccount> _searchResults = new();
    private UserAccount? _selectedCustomer;
    private string? _successMessage;
    private string? _errorMessage;
    private CancellationTokenSource? _searchCts;

    private async Task OnCustomerSearchInput(ChangeEventArgs e)
    {
        _customerSearch = e.Value?.ToString() ?? string.Empty;
        _searchResults.Clear();

        if (_customerSearch.Length < 2)
            return;

        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        _isSearching = true;
        StateHasChanged();

        try
        {
            await Task.Delay(300, token);
            if (token.IsCancellationRequested) return;

            var result = await UserService.SearchUsers(_customerSearch, token);
            if (result.IsSuccess && result.Value is not null)
                _searchResults = result.Value;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Customer search failed for term {Term}", _customerSearch);
        }
        finally
        {
            _isSearching = false;
            StateHasChanged();
        }
    }

    private void SelectCustomer(UserAccount user)
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
            CustomerId = _selectedCustomer.userId,
            AssignedUserId = _assignToSelf ? UserContext.UserId : null,
            Description = string.IsNullOrWhiteSpace(_description) ? null : _description.Trim()
        }, origin: TicketOrigin.Manual);

        if (result.IsSuccess)
        {
            _successMessage = "Ticket created! Redirecting to your inbox...";
            StateHasChanged();
            await Task.Delay(1200);
            Navigation.NavigateTo("/inbox");
        }
        else
        {
            Logger.LogError("Failed to create ticket: {Code} - {Description}",
                result.Error.Code, result.Error.Description);
            _errorMessage = "Something went wrong creating the ticket. Please try again.";
            _isSubmitting = false;
        }
    }

    private static string GetInitials(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "?";
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length >= 2
            ? $"{parts[0][0]}{parts[1][0]}".ToUpper()
            : name[0].ToString().ToUpper();
    }
}
