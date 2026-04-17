using CCP.Shared.ResultAbstraction;
using ChatService.Application.Interfaces;
using ChatService.Domain.Entities;
using ChatService.Domain.Interfaces;
using ChatService.Infrastructure.LLM.Prompts;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChatService.Infrastructure.LLM.Chat
{
    public class ChatService : IChatService
    {
        private readonly IChatClient _chatClient;
        private readonly ILogger<ChatService> _logger;
        private readonly IEmbeddingService _embeddingService;
        private readonly IFaqRepository _faqRepository;
        public ChatService([FromKeyedServices("qwen")] IChatClient chatClient,
                           IEmbeddingService embeddingService,
                           ILogger<ChatService> logger,
                           IFaqRepository faqRepository)
        {
            _chatClient = chatClient;
            _embeddingService = embeddingService;
            _logger = logger;
            _faqRepository = faqRepository;
        }

        public async Task<Result<MessageEntity>> GetChatBotResponseAsync(string userMessage, List<FaqEntity> mostRelevantFaqs, List<MessageEntity> History, Guid ConversationId, Guid OrgId, string OrgName)
        {
            try
            {
                var FullPrompt = ChatPrompts.Build(UserMessage: userMessage,
                                                   CompanyName: OrgName,
                                                   faqEntities: mostRelevantFaqs,
                                                   history: History);

                var response = await _chatClient.GetResponseAsync(FullPrompt);

                // Create a MessageEntity for the response
                var messageEntity = new MessageEntity
                {
                    Id = Guid.NewGuid(),
                    OrgId = OrgId,
                    ConversationId = ConversationId,
                    MessageInput = userMessage,
                    MessageOutput = response.Text,
                    IsFromUser = false,
                    CreatedAt = DateTime.UtcNow
                };

                return Result.Success(messageEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting chat response.");
                return Result.Failure<MessageEntity>(Error.Failure("ChatServiceError", "An error occurred while processing the chat response."));
            }
        }

    }
}
