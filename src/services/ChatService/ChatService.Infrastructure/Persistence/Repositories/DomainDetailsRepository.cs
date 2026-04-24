using CCP.Shared.ResultAbstraction;
using ChatService.Domain.Entities;
using ChatService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChatService.Infrastructure.Persistence.Repositories
{
    public class DomainDetailsRepository : IDomainDetailsRepository
    {
        private readonly ILogger<DomainDetailsRepository> _logger;
        private readonly ChatDbContext _context;

        public DomainDetailsRepository(ILogger<DomainDetailsRepository> logger, ChatDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<Result> AddDomainDetails(DomainDetails details)
        {
            try
            {
                await _context.DomainDetails.AddAsync(details);
                await _context.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding domain details for OrgId: {OrgId}, Domain: {Domain}", details.OrgId, details.Domain);
                return Result.Failure(Error.Failure(code: "AddDomainDetailsError", description: "An error occurred while adding domain details."));
            }
        }
        public async Task<Result> UpdateDomainDetails(DomainDetails details)
        {
            try
            {
                _context.DomainDetails.Update(details);
                await _context.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating domain details for OrgId: {OrgId}, Domain: {Domain}", details.OrgId, details.Domain);
                return Result.Failure(Error.Failure(code: "UpdateDomainDetailsError", description: "An error occurred while updating domain details."));
            }
        }

        public async Task<Result<DomainDetails?>> GetDomainDetailsByOrgId(Guid orgId)
        {
            try
            {
                var details = await _context.DomainDetails.FirstOrDefaultAsync(d => d.OrgId == orgId);
                if (details == null)
                {
                    return Result.Failure<DomainDetails?>(Error.Failure(code: "DomainDetailsNotFound", description: "Domain details not found for the given organization ID."));
                }
                return details;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving domain details for OrgId: {OrgId}", orgId);
                return Result.Failure<DomainDetails?>(Error.Failure(code: "GetDomainDetailsError", description: "An error occurred while retrieving domain details."));
            }
        }

        public async Task<Result<DomainDetails>> GetDomainDetailsBasedOnDomain(string domain)
        {
            try
            {
                var details = await _context.DomainDetails.SingleOrDefaultAsync(d => d.Domain.ToLower() == domain.ToLower());
                if (details == null)
                {
                    return Result.Failure<DomainDetails>(Error.Failure(code: "DomainDetailsNotFound", description: "Domain details not found for the given domain."));
                }
                return Result.Success(details);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving domain details for Domain: {Domain}", domain);
                return Result.Failure<DomainDetails>(Error.Failure(code: "GetDomainDetailsError", description: "An error occurred while retrieving domain details."));
            }
        }

        public Result<bool> IsDomainAllowed(string domain)
        {
            try
            {
                return _context.DomainDetails
                    .Where(d => d.Domain.ToLower() == domain.ToLower())
                    .Any();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if domain is allowed");
                return Result.Failure<bool>(Error.Failure(code: "IsDomainAllowedError", description: "An error occurred while checking if the domain is allowed."));
            }
        }
    }
}
