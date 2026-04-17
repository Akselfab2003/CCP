using CCP.Shared.ResultAbstraction;
using ChatService.Application.AuthContext;
using ChatService.Application.Interfaces;
using ChatService.Application.Services.Faq;
using ChatService.Domain.Entities;
using ChatService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ChatService.Application.Services.Chat
{
    public class ChatManagementService : IChatManagementService
    {
        private readonly ILogger<ChatManagementService> _logger;
        private readonly IConversationRepository _conversationRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IFaqManagementService _faqManagementService;
        private readonly IChatService _chatService;
        private readonly IActiveSession _activeSession;

        public ChatManagementService(ILogger<ChatManagementService> logger,
                                     IConversationRepository conversationRepository,
                                     IMessageRepository messageRepository,
                                     IFaqManagementService faqManagementService,
                                     IChatService chatService,
                                     IActiveSession activeSession)
        {
            _logger = logger;
            _conversationRepository = conversationRepository;
            _messageRepository = messageRepository;
            _faqManagementService = faqManagementService;
            _chatService = chatService;
            _activeSession = activeSession;
        }

        public async Task<Result<string>> GetChatResponseToMessage(string message, Guid? ConversationId)
        {
            try
            {
                if (ConversationId == null)
                {
                    return await CreateNewConversation(_activeSession.SessionId, message);
                }
                else
                {
                    var conversationResult = await _conversationRepository.GetConversationById(ConversationId.Value);
                    if (conversationResult.IsFailure)
                    {
                        return Result.Failure<string>(Error.Failure(code: "FailedToFindConversation", description: "Failed to find conversation with the provided ID."));
                    }
                    var Conversation = conversationResult.Value;
                    var ConversationHistory = Conversation.Messages;

                    var relevantFaq = await _faqManagementService.GetRelevantFaqAsync(message);
                    if (relevantFaq.IsFailure) return Result.Failure<string>(Error.Failure(code: "FailedToFindRelevantFaqs", description: "Failed to find relevant FAQs for the message."));

                    var MessageResponse = await _chatService.GetChatBotResponseAsync(message, relevantFaq.Value, ConversationHistory, Conversation.Id, _activeSession.OrgId, _activeSession.Host);
                    if (MessageResponse.IsFailure)
                    {
                        return Result.Failure<string>(Error.Failure(code: "FailedToGetChatResponse", description: "Failed to get chat response for the initial message."));
                    }

                    var AddedMessageToDb = await _messageRepository.AddMessage(MessageResponse.Value);
                    if (AddedMessageToDb.IsFailure)
                    {
                        return Result.Failure<string>(Error.Failure(code: "FailedToAddMessage", description: "Failed to add message to the database."));
                    }

                    return Result.Success(MessageResponse.Value.MessageOutput);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting chat response to message.");
                return Result.Failure<string>(Error.Failure(code: "ChatResponseError", description: "An error occurred while getting the chat response."));
            }
        }


        private async Task<Result<string>> CreateNewConversation(Guid SessionId, string InitialMessage)
        {
            try
            {
                var newConversation = new ConversationEntity
                {
                    Id = Guid.NewGuid(),
                    OrgId = _activeSession.OrgId,
                    SessionId = SessionId,
                    CreatedAt = DateTime.UtcNow,
                };

                var addConversationResult = await _conversationRepository.AddConversation(newConversation);

                if (addConversationResult.IsFailure)
                {
                    return Result.Failure<string>(Error.Failure(code: "FailedToCreateConversation", description: "Failed to create a new conversation."));
                }

                Result<List<FaqEntity>> RelevantFaqsResult = await _faqManagementService.GetRelevantFaqAsync(InitialMessage);
                if (RelevantFaqsResult.IsFailure) return Result.Failure<string>(Error.Failure(code: "FailedToFindRelevantFaqs", description: "Failed to find relevant FAQs for the initial message."));
                var MessageResponse = await _chatService.GetChatBotResponseAsync(InitialMessage, RelevantFaqsResult.Value, [], newConversation.Id, _activeSession.OrgId, _activeSession.Host);

                if (MessageResponse.IsFailure)
                {
                    return Result.Failure<string>(Error.Failure(code: "FailedToGetChatResponse", description: "Failed to get chat response for the initial message."));
                }

                var AddedMessageToDb = await _messageRepository.AddMessage(MessageResponse.Value);

                return Result.Success(MessageResponse.Value.MessageOutput);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating a new conversation.");
                return Result.Failure<string>(Error.Failure(code: "CreateConversationError", description: "An error occurred while creating the conversation."));
            }
        }
    }
}
