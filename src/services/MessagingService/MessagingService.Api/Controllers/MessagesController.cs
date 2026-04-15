using MessagingService.Api.Hubs;
using MessagingService.Domain.Contracts;
using MessagingService.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace MessagingService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(IMessageService messageService, IHubContext<ChatHub> hubContext, ILogger<MessagesController> logger)
    {
        _messageService = messageService;
        _hubContext = hubContext;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<MessageResponse>> CreateMessage(
        CreateMessageRequest request, CancellationToken cancellationToken)
    {
        var result = await _messageService.CreateMessageAsync(request, cancellationToken);

        if (!result.Success || result.Message is null)
            return BadRequest(result.ErrorMessage);

        // Broadcast to all connected clients watching this ticket
        await _hubContext.Clients
            .Group($"ticket-{result.Message.TicketId}")
            .SendAsync("ReceiveMessage", result.Message, cancellationToken);

        return CreatedAtAction(nameof(GetMessageById),
            new { messageId = result.Message.Id }, result.Message);
    }

    [HttpGet("ticket/{ticketId:int}")]
    public async Task<ActionResult<PagedMessagesResponse>> GetMessagesByTicketId(int ticketId, [FromQuery] int limit = 50, [FromQuery] int? beforeMessageId = null, CancellationToken cancellationToken = default)
    {
        if (ticketId <= 0)
            return BadRequest("TicketId must be greater than 0.");

        var result = await _messageService.GetMessagesByTicketIdAsync(ticketId, limit, beforeMessageId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{messageId:int}")]
    public async Task<ActionResult<MessageResponse>> GetMessageById(int messageId, CancellationToken cancellationToken)
    {
        var message = await _messageService.GetMessageByIdAsync(messageId, cancellationToken);

        if (message is null)
            return NotFound();

        return Ok(message);
    }

    [HttpPut("{messageId:int}")]
    public async Task<ActionResult<MessageResponse>> UpdateMessage(int messageId, UpdateMessageRequest request, CancellationToken cancellationToken)
    {
        var result = await _messageService.UpdateMessageAsync(messageId, request, cancellationToken);

        if (!result.Success || result.Message is null)
        {
            if (result.ErrorMessage == "Message not found.")
                return NotFound();

            return BadRequest(result.ErrorMessage);
        }

        // Broadcast the updated message to all connected clients
        await _hubContext.Clients
            .Group($"ticket-{result.Message.TicketId}")
            .SendAsync("MessageUpdated", result.Message, cancellationToken);

        return Ok(result.Message);
    }

    [HttpDelete("{messageId:int}")]
    public async Task<IActionResult> DeleteMessage(int messageId, CancellationToken cancellationToken)
    {
        var (deleted, ticketId) = await _messageService.SoftDeleteMessageAsync(messageId, cancellationToken);

        if (!deleted)
            return NotFound();

        // Broadcast deletion to all connected clients
        await _hubContext.Clients
            .Group($"ticket-{ticketId}")
            .SendAsync("MessageDeleted", messageId, cancellationToken);

        return NoContent();
    }
}
