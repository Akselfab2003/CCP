using CCP.Shared.AuthContext;
using CCP.Shared.ResultAbstraction;
using EmailService.Application.Interfaces;
using EmailService.Domain.Interfaces;
using EmailService.Domain.Requests;
using Microsoft.Extensions.Logging;

namespace EmailService.Application.Services
{
    public class TenantEmailConfigurationService : ITenantEmailConfigurationService
    {
        private readonly ILogger<TenantEmailConfigurationService> _logger;
        private readonly ITenantEmailConfigurationRepo _tenantEmailConfigurationRepo;
        private readonly ICurrentUser _currentUser;
        public TenantEmailConfigurationService(ILogger<TenantEmailConfigurationService> logger, ITenantEmailConfigurationRepo tenantEmailConfigurationRepo, ICurrentUser currentUser)
        {
            _logger = logger;
            _tenantEmailConfigurationRepo = tenantEmailConfigurationRepo;
            _currentUser = currentUser;
        }


        public async Task<Result> AddTenantEmailConfigurationAsync(AddTenantEmailConfigurationRequest request)
        {
            try
            {
                var tenantEmailConfiguration = new Domain.Models.TenantEmailConfiguration
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = _currentUser.OrganizationId,
                    DefaultSenderEmail = request.DefaultSenderEmail,
                    Domain = request.Domain
                };

                var addResult = await _tenantEmailConfigurationRepo.AddAsync(tenantEmailConfiguration);

                return addResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding tenant email configuration for organization {OrganizationId}", _currentUser.OrganizationId);

                return Result.Failure(Error.Failure(code: "AddTenantEmailConfigurationError", description: "An error occurred while adding tenant email configuration."));
            }

        }
    }
}
