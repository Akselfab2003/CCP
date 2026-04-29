using CCP.Shared.ResultAbstraction;
using EmailService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmailService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TenantEmailConfigurationController : Controller
    {
        private readonly ILogger<TenantEmailConfigurationController> _logger;
        private readonly ITenantEmailConfigurationService _tenantEmailConfigurationService;
        public TenantEmailConfigurationController(ILogger<TenantEmailConfigurationController> logger, ITenantEmailConfigurationService tenantEmailConfigurationService)
        {
            _logger = logger;
            _tenantEmailConfigurationService = tenantEmailConfigurationService;
        }

        [HttpPost("create")]
        public async Task<IResult> Create([FromQuery] string DefaultSenderEmail)
        {
            try
            {
                var result = await _tenantEmailConfigurationService.AddTenantEmailConfigurationAsync(DefaultSenderEmail);
                return result.IsSuccess
                    ? Results.Ok()
                    : result.ToProblemDetails();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating tenant email configuration for default sender email {DefaultSenderEmail}", DefaultSenderEmail);
                return Results.Problem("An unexpected error occurred while processing your request.");
            }
        }
        [HttpPut("update")]
        public async Task<IResult> Update([FromQuery] string DefaultSenderEmail)
        {
            try
            {
                var result = await _tenantEmailConfigurationService.UpdateTenantEmailConfigurationAsync(DefaultSenderEmail);
                return result.IsSuccess
                    ? Results.Ok()
                    : result.ToProblemDetails();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating tenant email configuration for default sender email {DefaultSenderEmail}", DefaultSenderEmail);
                return Results.Problem("An unexpected error occurred while processing your request.");
            }
        }

        [HttpGet("get")]
        public async Task<IResult> Get()
        {
            try
            {
                var result = await _tenantEmailConfigurationService.GetTenantEmailConfigurationAsync();
                if (!result.IsSuccess) return result.ToProblemDetails();
                if (result.Value == null) return Results.NoContent();
                return Results.Ok(result.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching tenant email configuration.");
                return Results.Problem("An unexpected error occurred while processing your request.");
            }
        }
    }
}
