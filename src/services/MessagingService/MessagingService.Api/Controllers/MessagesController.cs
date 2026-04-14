using MessagingService.Api.Hubs;
using MessagingService.Domain.Contracts;
using MessagingService.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Net.Http;
using System.Net.Http.Json;

namespace MessagingService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(IMessageService messageService, IHubContext<ChatHub> hubContext, IHttpClientFactory httpClientFactory, ILogger<MessagesController> logger)
    {
        _messageService = messageService;
        _hubContext = hubContext;
        _httpClientFactory = httpClientFactory;
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

        // Auto-update ticket status based on sender role (fire-and-forget).
        // IMPORTANT: capture role strings NOW while HttpContext is still alive —
        // User.Claims is unavailable after the request completes.
        if (!result.Message.IsInternalNote)
        {
            var roleClaimType = System.Security.Claims.ClaimTypes.Role;
            var senderRoles = User.Claims
                .Where(c => c.Type == roleClaimType)
                .Select(c => c.Value)
                .ToList();

            _ = UpdateTicketStatusForSenderAsync(result.Message.TicketId, senderRoles);
        }

        return CreatedAtAction(nameof(GetMessageById),
            new { messageId = result.Message.Id }, result.Message);
    }

    private async Task UpdateTicketStatusForSenderAsync(int ticketId, List<string> senderRoles)
    {
        try
        {
            // Determine new status from pre-captured roles.
            // Roles have "org." prefix from Keycloak (e.g. "org.Supporter").
            int? newStatus = null;
            if (senderRoles.Any(r => r.Contains("Supporter", StringComparison.OrdinalIgnoreCase)
                                  || r.Contains("Manager", StringComparison.OrdinalIgnoreCase)
                                  || r.Contains("Admin", StringComparison.OrdinalIgnoreCase)))
                newStatus = 1; // WaitingForCustomer
            else if (senderRoles.Any(r => r.Contains("Customer", StringComparison.OrdinalIgnoreCase)))
                newStatus = 2; // WaitingForSupport

            if (newStatus is null)
            {
                _logger.LogWarning("Could not determine sender role for ticket {TicketId} status update. Roles: [{Roles}]",
                    ticketId, string.Join(", ", senderRoles));
                return;
            }

            var http = _httpClientFactory.CreateClient("TicketService");
            if (http.BaseAddress is null)
            {
                _logger.LogWarning("TicketService HttpClient has no base address — skipping status update for ticket {TicketId}", ticketId);
                return;
            }

            // Guard: fetch current ticket status and skip if Closed (3) or Blocked (4)
            var currentResponse = await http.GetAsync($"/ticket/GetTicket/{ticketId}");
            if (currentResponse.IsSuccessStatusCode)
            {
                var current = await currentResponse.Content.ReadFromJsonAsync<TicketStatusCheckDto>();
                if (current?.Status is 3 or 4) // Closed or Blocked
                {
                    _logger.LogInformation("Skipping auto-status for ticket {TicketId} — current status is {Status} (Closed/Blocked)", ticketId, current.Status);
                    return;
                }
            }

            var response = await http.PatchAsJsonAsync(
                $"/ticket/{ticketId}/status",
                new { NewStatus = newStatus.Value });

            if (!response.IsSuccessStatusCode)
                _logger.LogWarning("Failed to update ticket {TicketId} status. HTTP {StatusCode}", ticketId, (int)response.StatusCode);
            else
                _logger.LogInformation("Auto-updated ticket {TicketId} status to {NewStatus}", ticketId, newStatus.Value);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Non-critical: failed to auto-update ticket {TicketId} status after message send", ticketId);
        }
    }

    // Minimal DTO just for reading the current ticket status in the guard check
    private sealed record TicketStatusCheckDto(int Status);

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
