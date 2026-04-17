using ChatService.Application.Interfaces;
using ChatService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ChatService.Application.Services.Chat
{
    public class ChatManagementService
    {
        private readonly ILogger<ChatManagementService> _logger;
        private readonly IFaqRepository _faqRepository;
        private readonly IEmbeddingService _embeddingService;
        private readonly IChatService _chatService;

        public ChatManagementService(ILogger<ChatManagementService> logger, IFaqRepository faqRepository, IEmbeddingService embeddingService, IChatService chatService)
        {
            _logger = logger;
            _faqRepository = faqRepository;
            _embeddingService = embeddingService;
            _chatService = chatService;
        }

        public async Task<string> HandleUserMessageAsync(string userMessage)
        {
            try
            {
                var chatResponseResult = await _chatService.GetChatResponseAsync(userMessage);
                if (chatResponseResult.IsSuccess)
                {
                    return chatResponseResult.Value;
                }
                else
                {
                    _logger.LogError("Chat service returned an error: {Error}", chatResponseResult.Error);
                    return "Sorry, I couldn't process your request at the moment. Please try again later.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while handling user message.");
                return "Sorry, something went wrong while processing your request. Please try again later.";
            }
        }
    }
}
