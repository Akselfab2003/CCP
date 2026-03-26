using Microsoft.Extensions.Logging;

namespace TicketService.Sdk.Services.Assignment
{
    internal class AssignmentApiService : IAssignmentService
    {
        private readonly ILogger<AssignmentApiService> _logger;
        private readonly IKiotaApiClient<TicketServiceClient> _client;

        private TicketServiceClient Client => _client.Client;

        public AssignmentApiService(ILogger<AssignmentApiService> logger, IKiotaApiClient<TicketServiceClient> client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task AssignTicketToUserAsync(int ticketId, Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                await Client.Assignment.Assign.PostAsync(new Models.UpdateTicketAssignmentRequest()
                {
                    TicketId = ticketId,
                    AssignToUserId = userId,
                });

                _logger.LogInformation("Successfully assigned ticket {TicketId} to user {UserId}", ticketId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning ticket {TicketId} to user {UserId}: {Message}", ticketId, userId, ex.Message);
                throw;
            }

        }
    }
}
