using MessagingService.Api.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace MessagingService.Api.Controllers;

public record TicketAssignmentNotificationRequest(int TicketId, Guid AssignedUserId);

[ApiController]
[Route("api/ticket-notifications")]
public class TicketNotificationController : ControllerBase
{
    private readonly IHubContext<ChatHub> _hubContext;

    public TicketNotificationController(IHubContext<ChatHub> hubContext)
    {
        _hubContext = hubContext;
    }

    [HttpPost("assignment-updated")]
    public async Task<IActionResult> AssignmentUpdated(
        [FromBody] TicketAssignmentNotificationRequest request,
        CancellationToken cancellationToken)
    {
        await _hubContext.Clients
            .Group($"ticket-{request.TicketId}")
            .SendAsync("TicketAssigned", request.TicketId, request.AssignedUserId, cancellationToken);

        return Ok();
    }
}
