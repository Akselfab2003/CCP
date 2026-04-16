using CCP.Shared.ResultAbstraction;

namespace ChatService.Application.Services.Session
{
    public interface ISessionManagement
    {
        Task<Result<Guid>> CreateSession(string Domain);
    }
}
