using CCP.Shared.ResultAbstraction;
using ChatService.Domain.Interfaces;
using IdentityService.Sdk.Services.Tenant;
using Microsoft.Extensions.Logging;

namespace ChatService.Application.Services
{
    public class SessionManagement : ISessionManagement
    {
        private readonly ILogger<SessionManagement> _logger;
        private readonly ISessionRepository _sessionRepo;
        private readonly ITenantService _tenantService;
        public SessionManagement(ILogger<SessionManagement> logger, ISessionRepository sessionRepo, ITenantService tenantService)
        {
            _logger = logger;
            _sessionRepo = sessionRepo;
            _tenantService = tenantService;
        }

        public async Task<Result> CreateSession(string Domain)
        {
            try
            {
                var tenantResult = await _tenantService.GetTenantDetailsAsync(tenantId: null, domain: Domain);

                if (tenantResult.IsFailure)
                    return Result.Failure(Error.Failure("TenantNotFound", $"No tenant found for domain {Domain}"));

                var Session = new Domain.Entities.SessionEntity()
                {
                    SessionId = Guid.NewGuid(),
                    OrganizationId = tenantResult.Value.OrgId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var result = await _sessionRepo.AddSession(Session);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while creating session");
                return Result.Failure(Error.Failure("SessionCreationFailed", "An error occurred while creating the session"));
            }
        }
    }
}
