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
        private readonly ICurrentUser _currentUser;

        public DomainServices(ILogger<DomainServices> logger, IDomainDetailsRepository repository, ICurrentUser currentUser)
        {
            _logger = logger;
            _repository = repository;
            _currentUser = currentUser;
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
    }
}
