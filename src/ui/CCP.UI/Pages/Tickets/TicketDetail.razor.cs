using CCP.Shared.UIContext;
using CCP.Shared.ValueObjects;
using Microsoft.AspNetCore.Components;
using TicketService.Sdk.Dtos;
using TicketService.Sdk.Services.Ticket;

namespace CCP.UI.Pages.Tickets;

public partial class TicketDetail : ComponentBase
{
    [Inject] private ITicketService TicketService { get; set; } = default!;
    [Inject] private IUIUserContext UserContext { get; set; } = default!;
    [Inject] private ILogger<TicketDetail> Logger { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    [Parameter] public int TicketId { get; set; }

    private TicketSdkDto? _ticket;
    private bool _isLoading = true;
    private string? _errorMessage;

    private enum TicketView { Manager, Supporter, Customer }
    private TicketView? _ticketView;

    protected override async Task OnParametersSetAsync()
    {
        if (!RendererInfo.IsInteractive)
            return;

        var result = await TicketService.GetTicket(TicketId);

        if (result.IsSuccess)
        {

            _ticket = result.Value;
            _ticketView = (UserContext.Role == UserRole.Manager || UserContext.Role == UserRole.Admin)
                  ? TicketView.Manager
                  : UserContext.IsInternalUser ? TicketView.Supporter : TicketView.Customer;


            if (_ticket.OrganizationId != UserContext.OrganizationId)
            {
                Logger.LogWarning("Unauthorized access attempt by user {UserId} to ticket {TicketId} in organization {OrganizationId}", UserContext.UserId, TicketId, _ticket.OrganizationId);
                _errorMessage = "You don't have access to this ticket.";
                _ticket = null; // Clear the ticket data to prevent display
                return;
            }



            if (_ticketView.Value == TicketView.Customer)
            {
                // Check if the customer is trying to access a ticket that doesn't belong to them
                if (_ticket.CustomerId != UserContext.UserId)
                {
                    Logger.LogWarning("Unauthorized access attempt by user {UserId} to ticket {TicketId}", UserContext.UserId, TicketId);
                    _errorMessage = "You don't have access to this ticket.";
                    _ticket = null; // Clear the ticket data to prevent display
                    return;
                }
            }
            else if (_ticketView.Value == TicketView.Supporter)
            {
                if (_ticket.AssignedUserId == null)
                {
                    // Unassigned ticket, supporters can view
                }
                else if (_ticket.AssignedUserId != UserContext.UserId)
                {
                    Logger.LogWarning("Unauthorized access attempt by supporter {UserId} to ticket {TicketId} assigned to {AssignedUserId}", UserContext.UserId, TicketId, _ticket.AssignedUserId);
                    _errorMessage = "You don't have access to this ticket.";
                    _ticket = null; // Clear the ticket data to prevent display
                    return;
                }
            }
            else if (_ticketView.Value == TicketView.Manager)
            {
                // Managers can view all tickets in their organization, so no additional checks needed here
            }
            else
            {
                Logger.LogError("TicketDetail failed to load ticket {TicketId}: {Error}", TicketId, result.Error);
                _errorMessage = "Ticket not found or you don't have access to it.";
            }

            _isLoading = false;
        }
    }
}
