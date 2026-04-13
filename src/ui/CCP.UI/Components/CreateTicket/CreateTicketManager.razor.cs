using CCP.Shared.UIContext;
using IdentityService.Sdk.Models;
using IdentityService.Sdk.Services.User;
using MessagingService.Sdk.Services;
using Microsoft.AspNetCore.Components;
using TicketService.Sdk.Services.Ticket;

namespace CCP.UI.Components.CreateTicket;

public partial class CreateTicketManager : ComponentBase
{
    [Inject] private ITicketService TicketService { get; set; } = default!;
    [Inject] private IMessageSdkService MessageSdkService { get; set; } = default!;
    [Inject] private IUserService UserService { get; set; } = default!;
    [Inject] private IUIUserContext UserContext { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ILogger<CreateTicketManager> Logger { get; set; } = default!;

    private string _title = string.Empty;
    private string _description = string.Empty;
    private bool _titleTouched;
    private bool _customerTouched;
    private bool _isSubmitting;
    private string? _successMessage;
    private string? _errorMessage;

    // Customer search
    private string _customerSearch = string.Empty;
    private bool _isSearchingCustomer;
    private List<UserAccount> _customerResults = new();
    private UserAccount? _selectedCustomer;
    private CancellationTokenSource? _customerSearchCts;

    // Supporter search
    private string _supporterSearch = string.Empty;
    private bool _isSearchingSupporter;
    private List<UserAccount> _supporterResults = new();
    private UserAccount? _selectedSupporter;
    private CancellationTokenSource? _supporterSearchCts;

    private async Task OnCustomerSearchInput(ChangeEventArgs e)
    {
        _customerSearch = e.Value?.ToString() ?? string.Empty;
        _customerResults.Clear();

        if (_customerSearch.Length < 2)
            return;

        _customerSearchCts?.Cancel();
        _customerSearchCts = new CancellationTokenSource();
        var token = _customerSearchCts.Token;

        _isSearchingCustomer = true;
        StateHasChanged();

        try
        {
            await Task.Delay(300, token);
            if (token.IsCancellationRequested) return;

            var result = await UserService.SearchUsers(_customerSearch, token);
            if (result.IsSuccess && result.Value is not null)
                _customerResults = result.Value;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Customer search failed for term {Term}", _customerSearch);
        }
        finally
        {
            _isSearchingCustomer = false;
            StateHasChanged();
        }
    }

    private async Task OnSupporterSearchInput(ChangeEventArgs e)
    {
        _supporterSearch = e.Value?.ToString() ?? string.Empty;
        _supporterResults.Clear();

        if (_supporterSearch.Length < 2)
            return;

        _supporterSearchCts?.Cancel();
        _supporterSearchCts = new CancellationTokenSource();
        var token = _supporterSearchCts.Token;

        _isSearchingSupporter = true;
        StateHasChanged();

        try
        {
            await Task.Delay(300, token);
            if (token.IsCancellationRequested) return;

            var result = await UserService.SearchUsers(_supporterSearch, token);
            if (result.IsSuccess && result.Value is not null)
                _supporterResults = result.Value;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Supporter search failed for term {Term}", _supporterSearch);
        }
        finally
        {
            _isSearchingSupporter = false;
            StateHasChanged();
        }
    }

    private void SelectCustomer(UserAccount user)
    {
        _selectedCustomer = user;
        _customerSearch = string.Empty;
        _customerResults.Clear();
        _customerTouched = true;
    }

    private void ClearCustomer()
    {
        _selectedCustomer = null;
        _customerSearch = string.Empty;
        _customerResults.Clear();
    }

    private void SelectSupporter(UserAccount user)
    {
        _selectedSupporter = user;
        _supporterSearch = string.Empty;
        _supporterResults.Clear();
    }

    private void ClearSupporter()
    {
        _selectedSupporter = null;
        _supporterSearch = string.Empty;
        _supporterResults.Clear();
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
            AssignedUserId = (_selectedSupporter?.userId)
        });

        if (result.IsSuccess)
        {
            if (!string.IsNullOrWhiteSpace(_description))
            {
                var messageResult = await MessageSdkService.CreateMessageAsync(
                    ticketId: result.Value,
                    organizationId: UserContext.OrganizationId,
                    userId: UserContext.UserId,
                    content: _description.Trim());

                if (messageResult.IsFailure)
                    Logger.LogWarning("Ticket created but failed to send description as message: {Error}",
                        messageResult.Error.Description);
            }

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
