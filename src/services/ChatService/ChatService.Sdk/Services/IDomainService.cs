using CCP.Shared.ResultAbstraction;

namespace ChatService.Sdk.Services
{
    public interface IDomainService
    {
        Task<Result> AddOrUpdateDomain(string domain);
        Task<Result<string>> GetDomain();
    }
}
