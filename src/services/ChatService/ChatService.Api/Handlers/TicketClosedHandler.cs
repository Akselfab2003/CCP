using CCP.Shared.AuthContext;
using CCP.Shared.Events;
using ChatService.Application.Services.Automated;

namespace ChatService.Api.Handlers
{
    public class TicketClosedHandler
    {
        private readonly ILogger<TicketClosedHandler> _logger;
        private readonly ICurrentUser _currentUser;
        private readonly IAutomaticMessageGeneration _automaticMessageGeneration;

        public TicketClosedHandler(ILogger<TicketClosedHandler> logger, ICurrentUser currentUser, IAutomaticMessageGeneration automaticMessageGeneration)
        {
            _logger = logger;
            _currentUser = currentUser;
            _automaticMessageGeneration = automaticMessageGeneration;
        }

        public void Handle(TicketClosed ticketClosed)
        {
            _logger.LogInformation("Received TicketClosed event for TicketId: {TicketId}, OrgId: {OrgId}, ClosedAt: {ClosedAt}",
                ticketClosed.TicketId, ticketClosed.OrgId, ticketClosed.ClosedAt);
            _currentUser.SetOrganizationId(ticketClosed.OrgId);
            _automaticMessageGeneration.TicketClosedAnalysis(ticketClosed.TicketId);
        }
    }
}
