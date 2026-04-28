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

    protected override async Task OnInitializedAsync()
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
        }
        else
        {
            Logger.LogError("TicketDetail failed to load ticket {TicketId}: {Error}", TicketId, result.Error);
            _errorMessage = "Ticket not found or you don't have access to it.";
        }

        _isLoading = false;
    }
}
