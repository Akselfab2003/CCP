using CCP.Shared.ResultAbstraction;
using ChatService.Domain.Entities;

namespace ChatService.Domain.Interfaces
{
    public interface ISessionRepository
    {
        Task<Result> AddSession(SessionEntity session, CancellationToken ct = default);
        Task<Result<SessionEntity>> GetSessionByIdAsync(Guid sessionId, CancellationToken ct = default);
    }
}
