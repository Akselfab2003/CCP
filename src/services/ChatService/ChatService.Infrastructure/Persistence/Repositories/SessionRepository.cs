using CCP.Shared.ResultAbstraction;
using ChatService.Domain.Entities;
using ChatService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChatService.Infrastructure.Persistence.Repositories
{
    public class SessionRepository : ISessionRepository
    {
        private readonly ChatDbContext _context;
        private readonly ILogger<SessionRepository> _logger;

        public SessionRepository(ChatDbContext context, ILogger<SessionRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result> AddSession(SessionEntity session, CancellationToken ct = default)
        {
            try
            {
                await _context.Sessions.AddAsync(session, ct);
                await _context.SaveChangesAsync(ct);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not add session with ID: {SessionId}", session.SessionId);
                return Result.Failure(Error.Failure("SessionAddFailed", "An error occurred while adding the session."));
            }
        }

        public async Task<Result<SessionEntity>> GetSessionByIdAsync(Guid sessionId, CancellationToken ct = default)
        {
            try
            {
                var session = await _context.Sessions.IgnoreQueryFilters().SingleOrDefaultAsync(s => s.SessionId == sessionId, ct);

                return session is null
                    ? Result.Failure<SessionEntity>(Error.NotFound("SessionNotFound", $"No session found with ID: {sessionId}"))
                    : Result.Success(session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not retrieve session with ID: {SessionId}", sessionId);
                return Result.Failure<SessionEntity>(Error.Failure("SessionRetrievalFailed", "An error occurred while retrieving the session."));
            }
        }
    }
}
