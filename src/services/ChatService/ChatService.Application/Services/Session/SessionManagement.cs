using CCP.Shared.ResultAbstraction;
using ChatService.Application.Services.Domain;
using ChatService.Domain.Entities;
using ChatService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ChatService.Application.Services.Session
{
    public class SessionManagement : ISessionManagement
    {
        private readonly ILogger<SessionManagement> _logger;
        private readonly ISessionRepository _sessionRepo;
        private readonly IDomainServices _domainServices;
        public SessionManagement(ILogger<SessionManagement> logger, ISessionRepository sessionRepo, IDomainServices domainServices)
        {
            _logger = logger;
            _sessionRepo = sessionRepo;
            _domainServices = domainServices;
        }

        public async Task<Result<Guid>> CreateSession(string Domain)
        {
            try
            {
                var Host = new Uri(Domain).Host;
                var validateDomain = _domainServices.IsDomainAllowed(Host);
                if (!validateDomain) return Result.Failure<Guid>(Error.Failure("InvalidDomain", $"The domain {Domain} is not allowed to create a session"));

                var domainDetailsResult = await _domainServices.GetDomainDetails(Host);
                if (domainDetailsResult is null || domainDetailsResult.IsFailure) return Result.Failure<Guid>(Error.NotFound("DomainNotFound", $"No details found for domain {Domain}"));

                var details = domainDetailsResult.Value;

                var Session = new SessionEntity()
                {
                    SessionId = Guid.NewGuid(),
                    OrganizationId = details.OrgId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var result = await _sessionRepo.AddSession(Session);

                return Session.SessionId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while creating session");
                return Result.Failure<Guid>(Error.Failure("SessionCreationFailed", "An error occurred while creating the session"));
            }
        }
    }
}
