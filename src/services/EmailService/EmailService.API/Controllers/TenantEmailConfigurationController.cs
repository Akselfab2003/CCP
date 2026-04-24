using CCP.Shared.ResultAbstraction;
using EmailService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EmailService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
    }
}
