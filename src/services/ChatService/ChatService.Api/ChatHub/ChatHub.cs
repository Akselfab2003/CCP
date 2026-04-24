using ChatService.Application.AuthContext;
using ChatService.Application.Services.Chat;
using ChatService.Application.Services.Domain;
using Microsoft.AspNetCore.SignalR;

namespace ChatService.Api.ChatHub
{
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;
        private readonly IDomainServices _domainServices;
        private readonly IActiveSession _activeSession;
        private readonly IChatManagementService _chatManagementService;
        private readonly IAuthParser _parser;
        public ChatHub(ILogger<ChatHub> logger, IDomainServices domainServices, IActiveSession activeSession, IAuthParser parser, IChatManagementService chatManagementService)
        {
            _logger = logger;
            _domainServices = domainServices;
            _activeSession = activeSession;
            _parser = parser;
            _chatManagementService = chatManagementService;
        }


        public async Task SendMessageToChatBot(string conversationId, string message)
        {
            var key = $"{_activeSession.Host}:{_activeSession.SessionId}";
            await Clients.Group(key).SendAsync("ReceiveTyping", conversationId, message);
            var response = await _chatManagementService.GetChatResponseToMessage(message, Guid.Parse(conversationId));
            if (response.IsFailure)
            {
                _logger.LogError("Failed to get chat response for message: {Message}, error: {Error}", message, response.Error);
                await Clients.Group(key).SendAsync("ReceiveMessage", conversationId, "Sorry, something went wrong while processing your message.");
                return;
            }
            await Clients.Group(key).SendAsync("ReceiveMessage", conversationId, response.Value);
        }


        public override async Task OnConnectedAsync()
        {
            var HttpContext = Context.GetHttpContext();

            var parseResult = await _parser.ParseContext(HttpContext!);
            if (parseResult == null || parseResult.IsFailure)
            {
                Context.Abort();
                return;
            }

            var validateConnectionResult = await _domainServices.ValidateConnection(_activeSession.SessionId, _activeSession.Host);

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
