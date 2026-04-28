using CCP.Shared.AuthContext;
using CCP.Shared.Events;
using ChatService.Application.Services.Automated;

namespace ChatService.Api.Handlers
{
    public class TicketCreatedHandler
    {
        private readonly ILogger<TicketCreatedHandler> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;


        public TicketCreatedHandler(ILogger<TicketCreatedHandler> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task HandleAsync(TicketCreated ticketCreated)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var _currentUser = scope.ServiceProvider.GetRequiredService<ICurrentUser>();
                var _serviceAccountOverrider = scope.ServiceProvider.GetRequiredService<ServiceAccountOverrider>();
                var _automaticMessageGeneration = scope.ServiceProvider.GetRequiredService<IAutomaticMessageGeneration>();
                _logger.LogInformation("Received TicketCreated event for TicketId: {TicketId}, OrgId: {OrgId}, CreatedAt: {CreatedAt}",
                    ticketCreated.TicketId, ticketCreated.OrgId, ticketCreated.CreatedAt);
                _currentUser.SetOrganizationId(ticketCreated.OrgId);
                _serviceAccountOverrider.SetOrganizationId(_currentUser.OrganizationId);
                await _automaticMessageGeneration.TicketCreatedAnalysis(ticketCreated.TicketId);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing TicketCreated event for TicketId: {TicketId}, OrgId: {OrgId}", ticketCreated.TicketId, ticketCreated.OrgId);
            }
        }
    }
}
