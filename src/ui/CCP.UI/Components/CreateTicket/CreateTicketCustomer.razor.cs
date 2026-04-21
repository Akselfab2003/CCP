using CCP.Shared.UIContext;
using MessagingService.Sdk.Services;
using Microsoft.AspNetCore.Components;

namespace CCP.UI.Components.CreateTicket;

public partial class CreateTicketCustomer : ComponentBase
{
    [Inject] private TicketService.Sdk.Services.Ticket.ITicketService TicketService { get; set; } = default!;
    [Inject] private IMessageSdkService MessageSdkService { get; set; } = default!;
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
            AssignedUserId = null
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

            _successMessage = "Ticket submitted! Redirecting to your inbox...";
            StateHasChanged();
            await Task.Delay(1200);
            Navigation.NavigateTo("/");
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
