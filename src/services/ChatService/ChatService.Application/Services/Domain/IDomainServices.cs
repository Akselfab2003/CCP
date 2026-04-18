using CCP.Shared.ResultAbstraction;
using ChatService.Domain.Entities;

namespace ChatService.Application.Services.Domain
{
    public interface IDomainServices
    {
        Task<Result> AddOrUpdateDomainDetails(string domain);
        Task<Result<DomainDetails>> GetDomainDetails(string domain);
        bool IsDomainAllowed(string Host);
    }
}
