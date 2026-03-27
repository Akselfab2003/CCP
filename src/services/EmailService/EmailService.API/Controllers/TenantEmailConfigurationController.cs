using CCP.Shared.ResultAbstraction;
using EmailService.Application.Interfaces;
using EmailService.Domain.Requests;
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
        public async Task<IResult> Create(AddTenantEmailConfigurationRequest request)
        {
            try
            {
                var result = await _tenantEmailConfigurationService.AddTenantEmailConfigurationAsync(request);
                return result.IsSuccess
                    ? Results.Ok()
                    : result.ToProblemDetails();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating tenant email configuration for domain {Domain}", request.Domain);
                return Results.Problem("An unexpected error occurred while processing your request.");
            }
        }
    }
}
