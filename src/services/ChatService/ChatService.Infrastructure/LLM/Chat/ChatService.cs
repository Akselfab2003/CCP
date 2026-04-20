using CCP.Shared.ResultAbstraction;
using ChatService.Application.Interfaces;
using ChatService.Domain.Entities;
using ChatService.Infrastructure.LLM.Prompts;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace ChatService.Infrastructure.LLM.Chat
{
    /// <summary>
    /// Service responsible for handling chat interactions with the chatbot.
    /// </summary>
    public class ChatService : IChatService
    {
        private readonly IChatClient _chatClient;
        private readonly ILogger<ChatService> _logger;

        public ChatService([FromKeyedServices("qwen")] IChatClient chatClient,
                           ILogger<ChatService> logger)
        {
            _chatClient = chatClient;
            _logger = logger;
        }

        public async Task<Result<MessageEntity>> GetChatBotResponseAsync(string userMessage, List<FaqEntity> mostRelevantFaqs, List<MessageEntity> History, Guid ConversationId, Guid OrgId, string OrgName)
        {
            try
            {
                var FullPrompt = ChatPrompts.Build(UserMessage: userMessage,
                                                   CompanyName: OrgName,
                                                   faqEntities: mostRelevantFaqs,
                                                   history: History);

                var AiTool = AIFunctionFactory.Create(() =>
                {
                    Console.WriteLine("Testing Chatbot tool calls ");
                    _logger.LogError("Chatbot tool was called to contact a supporter.");
                    test();
                    return "A supporer is on their way";
                }, "escalate_to_supporter", description: "escalate to a human");

                var options = new ChatOptions()
                {
                    Tools = [AiTool],
                };

                var response = await _chatClient.GetResponseAsync(FullPrompt, options);
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

        private void test()
        {
            _logger.LogInformation("Testing the chat service implementation.");

        }
    }
}
