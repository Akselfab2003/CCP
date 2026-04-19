using CCP.Shared.ResultAbstraction;
using ChatService.Domain.Entities;

namespace ChatService.Application.Services.Domain
{
    public interface IDomainServices
    {
        Task<Result> AddOrUpdateDomainDetails(string domain);
        Task<Result<DomainDetails>> GetDomainDetails(string domain);
        Task<Result<DomainDetails?>> GetDomainDetailsByOrgId();
        bool IsDomainAllowed(string Host);
        Task<Result<bool>> ValidateConnection(Guid sessionId, string Host);
    }
}
