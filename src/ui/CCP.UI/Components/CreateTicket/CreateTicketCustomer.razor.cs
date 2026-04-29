using CCP.Shared.UIContext;
using CCP.Shared.ValueObjects;
using Microsoft.AspNetCore.Components;

namespace CCP.UI.Components.CreateTicket;

public partial class CreateTicketCustomer : ComponentBase
{
    [Inject] private TicketService.Sdk.Services.Ticket.ITicketService TicketService { get; set; } = default!;
    [Inject] private IUIUserContext UserContext { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;
    [Inject] private ILogger<CreateTicketCustomer> Logger { get; set; } = default!;

    private string _title = string.Empty;
    private string _description = string.Empty;
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

        var result = await TicketService.CreateTicket(new TicketService.Sdk.Dtos.CreateTicketRequestDto()
        {
            Title = _title.Trim(),
            CustomerId = UserContext.UserId,
            AssignedUserId = null,
            Description = string.IsNullOrWhiteSpace(_description) ? null : _description.Trim()
        }, origin: TicketOrigin.Manual);

        if (result.IsSuccess)
        {
            _successMessage = "Ticket submitted! Redirecting to your tickets...";
            StateHasChanged();
            await Task.Delay(1200);
            Navigation.NavigateTo("/my-tickets");
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
