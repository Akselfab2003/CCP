using CCP.Shared.Events;
using CCP.Shared.ResultAbstraction;
using EmailService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EmailService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailEventController : ControllerBase
    {
        private readonly ILogger<EmailEventController> _logger;
        private readonly IQueuePublisherService _queuePublisherService;
        public EmailEventController(ILogger<EmailEventController> logger, IQueuePublisherService queuePublisherService)
        {
            _logger = logger;
            _queuePublisherService = queuePublisherService;
        }

        [HttpPost]
        public async Task<IResult> PostEmailEvent([FromBody] DovecotEvent emailEvent)
        {
            try
            {
                _logger.LogInformation("Received email event: {event}", emailEvent);
                var result = await _queuePublisherService.PublishEmailMessageAsync(emailEvent);
                return result.IsSuccess ? Results.Ok() : result.ToProblemDetails();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the email event.");
                return Results.Problem(detail: $"An exception occurred while processing the email event: {ex.Message}", statusCode: 500);
            }
        }
    }
}
