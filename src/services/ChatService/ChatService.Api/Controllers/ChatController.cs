using ChatService.Interfaces;
using ChatService.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }
        [HttpPost("test")]
        public ActionResult Test([FromBody] ChatRequest request)
        {
            return Ok(new { message = "test virker", input = request.Message });
        }

        [HttpPost("send")]
        public async Task<ActionResult<ChatResponse>> Post(
            ChatRequest request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest("Message må ikke være tom.");

            var token = Request.Headers.Authorization
                .ToString()
                .Replace("Bearer ", string.Empty);

            var response = await _chatService.HandleAsync(request, token, ct);

            response.Reply = response.Reply
                .Replace("[OPRET_TICKET]", string.Empty)
                .Trim();

            return Ok(response);
        }
    }
}
