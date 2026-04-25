using CCP.Shared.AuthContext;
using CCP.Shared.Events;
using ChatService.Application.Services.Automated;

namespace ChatService.Api.Handlers
{
    public class TicketMessageReceivedHandler
    {
        private readonly ILogger<TicketMessageReceivedHandler> _logger;
        private readonly ICurrentUser _currentUser;
        private readonly IAutomaticMessageGeneration _automaticMessageGeneration;

        public TicketMessageReceivedHandler(ILogger<TicketMessageReceivedHandler> logger, ICurrentUser currentUser, IAutomaticMessageGeneration automaticMessageGeneration)
        {
            _logger = logger;
            _currentUser = currentUser;
            _automaticMessageGeneration = automaticMessageGeneration;
        }

        public void Handle(TicketMessageReceived ticketMessageReceived)
        {
            _logger.LogInformation("Received TicketMessageReceived event for TicketId: {TicketId}, OrgId: {OrgId}, ReceivedAt: {ReceivedAt}",
                ticketMessageReceived.TicketId, ticketMessageReceived.OrgId, ticketMessageReceived.ReceivedAt);
            _currentUser.SetOrganizationId(ticketMessageReceived.OrgId);
            _automaticMessageGeneration.NewMessageAddedToTicketAnalysis(ticketMessageReceived.TicketId);
        }
    }
}
