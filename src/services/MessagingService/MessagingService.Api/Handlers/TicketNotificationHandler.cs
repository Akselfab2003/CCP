using CCP.Shared.Events;
using MessagingService.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace MessagingService.Api.Handlers
{
    public class TicketNotificationHandler
    {
        private readonly ILogger<TicketNotificationHandler> _logger;
        private readonly IHubContext<ChatHub> _hubContext;

        public TicketNotificationHandler(ILogger<TicketNotificationHandler> logger, IHubContext<ChatHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
        }

        public async Task HandleTicketAssignmentUpdated(TicketAssignmentUpdated notification, CancellationToken cancellationToken)
        {
            try
            {
                await _hubContext.Clients.Group($"ticket-{notification.ticketId}")
                                         .SendAsync("TicketAssigned", notification.ticketId, notification.assignedUserId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling TicketAssignmentUpdated event for ticketId: {TicketId}", notification.ticketId);
            }
        }
    }
}
