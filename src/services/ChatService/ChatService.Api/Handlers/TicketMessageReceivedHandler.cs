using CCP.Shared.AuthContext;
using CCP.Shared.Events;
using ChatService.Application.Services.Automated;

namespace ChatService.Api.Handlers
{
    public class TicketMessageReceivedHandler
    {
        private readonly ILogger<TicketMessageReceivedHandler> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public TicketMessageReceivedHandler(ILogger<TicketMessageReceivedHandler> logger, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task Handle(TicketMessageReceived ticketMessageReceived)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                ICurrentUser currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUser>();
                ServiceAccountOverrider serviceAccountOverrider = scope.ServiceProvider.GetRequiredService<ServiceAccountOverrider>();
                IAutomaticMessageGeneration automaticMessageGeneration = scope.ServiceProvider.GetRequiredService<IAutomaticMessageGeneration>();

                _logger.LogInformation("Received TicketMessageReceived event for TicketId: {TicketId}, OrgId: {OrgId}, ReceivedAt: {ReceivedAt}",
                    ticketMessageReceived.TicketId, ticketMessageReceived.OrgId, ticketMessageReceived.ReceivedAt);
                currentUser.SetOrganizationId(ticketMessageReceived.OrgId);
                serviceAccountOverrider.SetOrganizationId(ticketMessageReceived.OrgId);
                await automaticMessageGeneration.NewMessageAddedToTicketAnalysis(ticketMessageReceived.TicketId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling TicketMessageReceived event for TicketId: {TicketId}, OrgId: {OrgId}",
                    ticketMessageReceived.TicketId, ticketMessageReceived.OrgId);
            }
        }
    }
}
