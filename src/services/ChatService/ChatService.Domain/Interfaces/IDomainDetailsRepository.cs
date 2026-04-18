using CCP.Shared.ResultAbstraction;
using ChatService.Domain.Entities;

namespace ChatService.Domain.Interfaces
{
    public interface IDomainDetailsRepository
    {
        Task<Result> AddDomainDetails(DomainDetails details);
        Task<Result<DomainDetails>> GetDomainDetailsBasedOnDomain(string domain);
        Task<Result<DomainDetails>> GetDomainDetailsByOrgId(Guid orgId);
        Result<bool> IsDomainAllowed(string domain);
    }
}
