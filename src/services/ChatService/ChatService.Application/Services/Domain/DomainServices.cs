using CCP.Shared.AuthContext;
using CCP.Shared.ResultAbstraction;
using ChatService.Domain.Entities;
using ChatService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ChatService.Application.Services.Domain
{
    public class DomainServices : IDomainServices
    {
        private readonly ILogger<DomainServices> _logger;
        private readonly IDomainDetailsRepository _repository;
        private readonly ISessionRepository _sessionRepository;
        private readonly ICurrentUser _currentUser;

        public DomainServices(ILogger<DomainServices> logger, IDomainDetailsRepository repository, ICurrentUser currentUser, ISessionRepository sessionRepository)
        {
            _logger = logger;
            _repository = repository;
            _currentUser = currentUser;
            _sessionRepository = sessionRepository;
        }

        public async Task<Result> AddOrUpdateDomainDetails(string domain)
        {
            var orgId = _currentUser.OrganizationId;
            var existingDetailsResult = await _repository.GetDomainDetailsBasedOnDomain(domain);
            if (existingDetailsResult.IsSuccess)
            {
                var existingDetails = existingDetailsResult.Value;
                existingDetails.Domain = domain;
                return await _repository.AddDomainDetails(existingDetails);
            }
            else
            {
                var newDetails = new DomainDetails
                {
                    Id = Guid.NewGuid(),
                    OrgId = orgId,
                    Domain = domain,
                    CreatedAt = DateTime.UtcNow,
                };
                return await _repository.AddDomainDetails(newDetails);
            }
        }

        public async Task<Result<DomainDetails>> GetDomainDetails(string domain)
        {
            try
            {
                return await _repository.GetDomainDetailsBasedOnDomain(domain);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting domain details");
                return Result.Failure<DomainDetails>(Error.Failure(code: "DomainDetailsError", description: "An error occurred while retrieving domain details"));
            }
        }

        public bool IsDomainAllowed(string Host)
        {
            try
            {
                var detailsResult = _repository.IsDomainAllowed(Host);
                if (detailsResult.IsFailure) return false;

                if (detailsResult.IsSuccess) return detailsResult.Value;

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if domain is allowed");
                return false;
            }
        }


        public async Task<Result<bool>> ValidateConnection(Guid sessionId, string Host)
        {
            try
            {
                var isDomainAllowed = IsDomainAllowed(Host);
                if (!isDomainAllowed)
                {
                    return Result.Failure<bool>(Error.Failure(code: "DomainNotAllowed", description: "The domain is not allowed"));
                }
                var sessionResult = await _sessionRepository.GetSessionByIdAsync(sessionId);
                var domainDetailsResult = await _repository.GetDomainDetailsBasedOnDomain(Host);

                if (sessionResult is null || sessionResult.IsFailure) return Result.Failure<bool>(Error.NotFound("SessionNotFound", $"No session found with ID {sessionId}"));
                var session = sessionResult.Value;

                if (domainDetailsResult is null || domainDetailsResult.IsFailure) return Result.Failure<bool>(Error.NotFound("DomainDetailsNotFound", $"No domain details found for domain {Host}"));

                var domain = domainDetailsResult.Value;

                if (session.OrganizationId != domain.OrgId)
                {
                    return Result.Failure<bool>(Error.Failure(code: "OrgIdMismatch", description: "The session's organization ID does not match the domain's organization ID"));
                }

                return Result.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while validating session");
                return Result.Failure<bool>(Error.Failure(code: "ValidationError", description: "An error occurred while validating the session"));
            }
        }
    }
}
