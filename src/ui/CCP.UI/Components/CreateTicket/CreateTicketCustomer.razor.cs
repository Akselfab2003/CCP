using CCP.Shared.UIContext;
using Microsoft.AspNetCore.Components;
using TicketService.Sdk.Services.TicketSdk;

namespace CCP.UI.Components.CreateTicket;

public partial class CreateTicketCustomer : ComponentBase
{
    [Inject] private ITicketSdkService TicketSdkService { get; set; } = default!;
    [Inject] private IUIUserContext UserContext { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ILogger<CreateTicketCustomer> Logger { get; set; } = default!;

    private string _title = string.Empty;
    private bool _titleTouched;
    private bool _isSubmitting;
    private string? _successMessage;
    private string? _errorMessage;

    private async Task HandleSubmitAsync()
    {
        _titleTouched = true;
        _errorMessage = null;

        if (string.IsNullOrWhiteSpace(_title))
            return;

        _isSubmitting = true;

        var result = await TicketSdkService.CreateTicketAsync(
            title: _title.Trim(),
            customerId: UserContext.UserId,
            assignedUserId: null);

        if (result.IsSuccess)
        {
            _successMessage = "Ticket submitted! Redirecting to your inbox...";
            StateHasChanged();
            await Task.Delay(1200);
            Navigation.NavigateTo("/inbox");
        }
        else
        {
            Logger.LogError("Failed to create ticket: {Code} - {Description}",
                result.Error.Code, result.Error.Description);
            _errorMessage = "Something went wrong submitting your ticket. Please try again.";
            _isSubmitting = false;
        }
    }
}
