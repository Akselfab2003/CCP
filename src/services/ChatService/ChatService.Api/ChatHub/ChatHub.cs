using ChatService.Application.AuthContext;
using ChatService.Application.Services.Domain;
using Microsoft.AspNetCore.SignalR;

namespace ChatService.Api.ChatHub
{
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;
        private readonly IDomainServices _domainServices;
        private readonly IActiveSession _activeSession;
        public ChatHub(ILogger<ChatHub> logger, IDomainServices domainServices, IActiveSession activeSession)
        {
            _logger = logger;
            _domainServices = domainServices;
            _activeSession = activeSession;
        }

        public override async Task OnConnectedAsync()
        {
            var HttpContext = Context.GetHttpContext();
            if (HttpContext == null) Context.Abort();

            var origin = HttpContext!.Request.Headers.Origin.First();
            if (!HttpContext.Request.Cookies.TryGetValue("SessionId", out var SessionIdCookie)) Context.Abort();
            var host = new Uri(origin!).Host;
            var sessionId = Guid.Parse(SessionIdCookie!);
            var validateConnectionResult = await _domainServices.ValidateConnection(sessionId, host);

            if (validateConnectionResult is null || validateConnectionResult.IsFailure)
            {
                Context.Abort();
            }
            else if (validateConnectionResult.Value)
            {
                var key = $"{_activeSession.Host}:{_activeSession.SessionId}";
                await Groups.AddToGroupAsync(Context.ConnectionId, key);
                await base.OnConnectedAsync();
            }
        }
    }
}
