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


            var cookie = HttpContext!.Request.Cookies["SessionId"];

            var origin = HttpContext!.Request.Headers["Origin"].FirstOrDefault();
            if (origin == null || cookie == null) Context.Abort();

            var host = new Uri(origin!).Host;
            var validateConnectionResult = await _domainServices.ValidateConnection(Guid.Parse(cookie!), host);
            _activeSession.SetSessionId(Guid.Parse(cookie!));
            var domainDetails = await _domainServices.GetDomainDetails(host);
            if (domainDetails != null && domainDetails.IsSuccess)
            {
                _activeSession.SetHost(host);
                _activeSession.SetOrgId(domainDetails.Value.OrgId);
            }

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
