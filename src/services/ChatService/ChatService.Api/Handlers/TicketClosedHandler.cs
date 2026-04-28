using CCP.Shared.AuthContext;
using CCP.Shared.Events;
using ChatService.Application.Services.Automated;

namespace ChatService.Api.Handlers
{
    public class TicketClosedHandler
    {
        private readonly ILogger<TicketClosedHandler> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public TicketClosedHandler(ILogger<TicketClosedHandler> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Handle(TicketClosed ticketClosed)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                ICurrentUser currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUser>();
                ServiceAccountOverrider serviceAccountOverrider = scope.ServiceProvider.GetRequiredService<ServiceAccountOverrider>();
                IAutomaticMessageGeneration automaticMessageGeneration = scope.ServiceProvider.GetRequiredService<IAutomaticMessageGeneration>();

                _logger.LogInformation("Received TicketClosed event for TicketId: {TicketId}, OrgId: {OrgId}, ClosedAt: {ClosedAt}", ticketClosed.TicketId, ticketClosed.OrgId, ticketClosed.ClosedAt);
                currentUser.SetOrganizationId(ticketClosed.OrgId);
                serviceAccountOverrider.SetOrganizationId(ticketClosed.OrgId);
                await automaticMessageGeneration.TicketClosedAnalysis(ticketClosed.TicketId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling TicketClosed event for TicketId: {TicketId}, OrgId: {OrgId}", ticketClosed.TicketId, ticketClosed.OrgId);
            }
        }
    }
}
