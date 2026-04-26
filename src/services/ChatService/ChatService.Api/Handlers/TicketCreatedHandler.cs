using CCP.Shared.AuthContext;
using CCP.Shared.Events;
using ChatService.Application.Services.Automated;

namespace ChatService.Api.Handlers
{
    public class TicketCreatedHandler
    {
        private readonly ILogger<TicketCreatedHandler> _logger;
        private readonly IAutomaticMessageGeneration _automaticMessageGeneration;
        private readonly ICurrentUser _currentUser;
        private readonly ServiceAccountOverrider _serviceAccountOverrider;

        public TicketCreatedHandler(ILogger<TicketCreatedHandler> logger, IAutomaticMessageGeneration automaticMessageGeneration, ICurrentUser currentUser, ServiceAccountOverrider serviceAccountOverrider)
        {
            _logger = logger;
            _automaticMessageGeneration = automaticMessageGeneration;
            _currentUser = currentUser;
            _serviceAccountOverrider = serviceAccountOverrider;
        }

        public void Handle(TicketCreated ticketCreated)
        {
            _logger.LogInformation("Received TicketCreated event for TicketId: {TicketId}, OrgId: {OrgId}, CreatedAt: {CreatedAt}",
                ticketCreated.TicketId, ticketCreated.OrgId, ticketCreated.CreatedAt);

            _currentUser.SetOrganizationId(ticketCreated.OrgId);
            _serviceAccountOverrider.SetOrganizationId(_currentUser.OrganizationId);

            _automaticMessageGeneration.TicketCreatedAnalysis(ticketCreated.TicketId);
        }
    }
}
