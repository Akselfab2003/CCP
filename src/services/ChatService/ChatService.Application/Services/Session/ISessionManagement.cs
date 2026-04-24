using CCP.Shared.ResultAbstraction;
using ChatService.Domain.Entities;

namespace ChatService.Application.Services.Session
{
    public interface ISessionManagement
    {
        Task<Result<Guid>> CreateSession(string Domain);
        Task<Result<SessionEntity>> GetSessionDetails(Guid SessionId);
    }
}
