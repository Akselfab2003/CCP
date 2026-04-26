using CCP.Shared.AuthContext;
using CCP.Shared.Events;
using ChatService.Application.Services.Automated;

namespace ChatService.Api.Handlers
{
    public class TicketClosedHandler
    {
        private readonly ILogger<TicketClosedHandler> _logger;
        private readonly IAutomaticMessageGeneration _automaticMessageGeneration;
        private readonly ICurrentUser _currentUser;
        private readonly ServiceAccountOverrider _serviceAccountOverrider;

        public TicketClosedHandler(ILogger<TicketClosedHandler> logger, ICurrentUser currentUser, IAutomaticMessageGeneration automaticMessageGeneration, ServiceAccountOverrider serviceAccountOverrider)
        {
            _logger = logger;
            _currentUser = currentUser;
            _automaticMessageGeneration = automaticMessageGeneration;
            _serviceAccountOverrider = serviceAccountOverrider;
        }

        public void Handle(TicketClosed ticketClosed)
        {
            _logger.LogInformation("Received TicketClosed event for TicketId: {TicketId}, OrgId: {OrgId}, ClosedAt: {ClosedAt}",
                ticketClosed.TicketId, ticketClosed.OrgId, ticketClosed.ClosedAt);
            _currentUser.SetOrganizationId(ticketClosed.OrgId);
            _serviceAccountOverrider.SetOrganizationId(ticketClosed.OrgId);
            _automaticMessageGeneration.TicketClosedAnalysis(ticketClosed.TicketId);
        }
    }
}
