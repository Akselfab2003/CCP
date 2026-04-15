using ChatService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ChatService.Application.Services
{
    public class SessionManagement
    {
        private readonly ILogger<SessionManagement> _logger;
        private readonly ISessionRepository _sessionRepo;

        public SessionManagement(ILogger<SessionManagement> logger, ISessionRepository sessionRepo)
        {
            _logger = logger;
            _sessionRepo = sessionRepo;
        }


    }
}
