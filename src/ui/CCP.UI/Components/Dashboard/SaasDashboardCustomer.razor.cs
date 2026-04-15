using CCP.Shared.UIContext;
using CCP.Shared.ValueObjects;
using Microsoft.AspNetCore.Components;
using TicketService.Sdk.Services.Ticket;

namespace CCP.UI.Components.Dashboard;

public partial class SaasDashboardCustomer : ComponentBase
{
    [Inject] private ITicketService TicketService { get; set; } = default!;
    [Inject] private IUIUserContext UserContext { get; set; } = default!;
    [Inject] private ILogger<SaasDashboardCustomer> Logger { get; set; } = default!;

    private int? _openCount;
    private int? _waitingForCustomerCount;
    private int? _waitingForSupporterCount;

    protected override async Task OnInitializedAsync()
    {
        if (!RendererInfo.IsInteractive)
            return;

        await LoadTicketStatsAsync();
    }

    private async Task LoadTicketStatsAsync()
    {
        var result = await TicketService.GetTickets(CustomerId: UserContext.UserId);

        if (result.IsFailure || result.Value is null)
        {
            Logger.LogError("Failed to load customer tickets for dashboard: {Error}", result.Error);
            return;
        }

        var tickets = result.Value;

        _openCount = tickets.Count(t => t.Status != (int)TicketStatus.Closed);
        _waitingForCustomerCount = tickets.Count(t => t.Status == (int)TicketStatus.WaitingForCustomer);
        _waitingForSupporterCount = tickets.Count(t => t.Status == (int)TicketStatus.WaitingForSupport);
    }
}
