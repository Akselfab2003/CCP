using CCP.Shared.ResultAbstraction;
using ChatService.Application.AuthContext;
using ChatService.Application.Interfaces;
using ChatService.Application.Services.Domain;
using ChatService.Application.Services.Faq;
using ChatService.Domain.Entities;
using ChatService.Domain.Interfaces;
using MessagingService.Sdk.Services;
using Microsoft.AspNetCore.SignalR;
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
        private readonly IHubContext<ChatHub.ChatHub> _chatHub;
        private readonly IDomainServices _domainServices;
        private readonly IMessageSdkService _messageSdkService;
        public ChatManagementService(ILogger<ChatManagementService> logger,
                                     IConversationRepository conversationRepository,
                                     IMessageRepository messageRepository,
                                     IFaqManagementService faqManagementService,
                                     IChatService chatService,
                                     IActiveSession activeSession,
                                     IHubContext<ChatHub.ChatHub> chatHub,
                                     IDomainServices domainServices,
                                     IMessageSdkService messageSdkService)
        {
            _logger = logger;
            _conversationRepository = conversationRepository;
            _messageRepository = messageRepository;
            _faqManagementService = faqManagementService;
            _chatService = chatService;
            _activeSession = activeSession;
            _chatHub = chatHub;
            _domainServices = domainServices;
            _messageSdkService = messageSdkService;
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

                    if (Conversation.IsEscalated)
                    {
                        await _messageRepository.AddMessage(new MessageEntity
                        {
                            Id = Guid.NewGuid(),
                            ConversationId = Conversation.Id,
                            IsFromUser = true,
                            OrgId = _activeSession.OrgId,
                            Message = message,
                            CreatedAt = DateTime.UtcNow,
                        });

                        if (Conversation.EscalatedTicketId != null)
                        {
                            await _messageSdkService.CreateMessageAsync((int)Conversation.EscalatedTicketId, Conversation.OrgId, null, message, false);
                        }

                        return "";
                    }



                    var messagesResult = await _messageRepository.GetMessagesByConversationId(Conversation.Id);
                    if (messagesResult.IsFailure)
                    {
                        return Result.Failure<string>(Error.Failure(code: "FailedToGetMessages", description: "Failed to get messages for the conversation."));
                    }
                    var ConversationHistory = messagesResult.Value;

                    var relevantFaq = await _faqManagementService.GetRelevantFaqAsync(message);
                    if (relevantFaq.IsFailure) return Result.Failure<string>(Error.Failure(code: "FailedToFindRelevantFaqs", description: "Failed to find relevant FAQs for the message."));

                    var addUserMessageToDb = await _messageRepository.AddMessage(new MessageEntity
                    {
                        Id = Guid.NewGuid(),
                        ConversationId = Conversation.Id,
                        IsFromUser = true,
                        OrgId = _activeSession.OrgId,
                        Message = message,
                        CreatedAt = DateTime.UtcNow,
                    });

                    if (addUserMessageToDb.IsFailure)
                    {
                        return Result.Failure<string>(Error.Failure(code: "FailedToAddMessage", description: "Failed to add message to the database."));
                    }


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

                    return Result.Success(MessageResponse.Value.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting chat response to message.");
                return Result.Failure<string>(Error.Failure(code: "ChatResponseError", description: "An error occurred while getting the chat response."));
            }
        }

        public async Task<Result<Guid>> CreateConversation(Guid SessionId)
        {
            try
            {
                var newConversation = new ConversationEntity
                {
                    Id = Guid.NewGuid(),
                    OrgId = _activeSession.OrgId,
                    SessionId = SessionId,
                    EscalatedTicketId = null,
                    IsEscalated = false,
                    CreatedAt = DateTime.UtcNow,
                };
                var addConversationResult = await _conversationRepository.AddConversation(newConversation);
                if (addConversationResult.IsFailure)
                {
                    return Result.Failure<Guid>(Error.Failure(code: "FailedToCreateConversation", description: "Failed to create a new conversation."));
                }
                return Result.Success(newConversation.Id);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating a new conversation.");
                return Result.Failure<Guid>(Error.Failure(code: "CreateConversationError", description: "An error occurred while creating the conversation."));
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
                var addUserMessageToDb = await _messageRepository.AddMessage(new MessageEntity
                {
                    Id = Guid.NewGuid(),
                    ConversationId = newConversation.Id,
                    IsFromUser = true,
                    OrgId = _activeSession.OrgId,
                    Message = InitialMessage,
                    CreatedAt = DateTime.UtcNow,
                });

                if (addUserMessageToDb.IsFailure)
                {
                    return Result.Failure<string>(Error.Failure(code: "FailedToAddMessage", description: "Failed to add message to the database."));
                }


                Result<List<FaqEntity>> RelevantFaqsResult = await _faqManagementService.GetRelevantFaqAsync(InitialMessage);
                if (RelevantFaqsResult.IsFailure) return Result.Failure<string>(Error.Failure(code: "FailedToFindRelevantFaqs", description: "Failed to find relevant FAQs for the initial message."));
                var MessageResponse = await _chatService.GetChatBotResponseAsync(InitialMessage, RelevantFaqsResult.Value, [], newConversation.Id, _activeSession.OrgId, _activeSession.Host);

                if (MessageResponse.IsFailure)
                {
                    return Result.Failure<string>(Error.Failure(code: "FailedToGetChatResponse", description: "Failed to get chat response for the initial message."));
                }

                var AddedMessageToDb = await _messageRepository.AddMessage(MessageResponse.Value);

                return Result.Success(MessageResponse.Value.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating a new conversation.");
                return Result.Failure<string>(Error.Failure(code: "CreateConversationError", description: "An error occurred while creating the conversation."));
            }
        }
        public async Task<Result> SendMessageFromSupportToConversation(int ticketId, string message)
        {
            try
            {
                var conversationResult = await _conversationRepository.GetConversationsByTicketId(ticketId);

                if (conversationResult.IsFailure)
                {
                    return Result.Failure(Error.Failure(code: "FailedToFindConversation", description: "Failed to find conversation for the provided ticket ID."));
                }

                var conversation = conversationResult.Value;

                var addMessageResult = await _messageRepository.AddMessage(new MessageEntity
                {
                    Id = Guid.NewGuid(),
                    ConversationId = conversation.Id,
                    IsFromUser = false,
                    OrgId = conversation.OrgId,
                    Message = message,
                    CreatedAt = DateTime.UtcNow,
                });

                if (addMessageResult.IsFailure)
                {
                    return Result.Failure(Error.Failure(code: "FailedToAddMessage", description: "Failed to add message to the database."));
                }
                var domainResult = await _domainServices.GetDomainDetailsByOrgId();
                if (domainResult.IsFailure)
                    return domainResult;


                await _chatHub.Clients.Group($"{domainResult.Value.Domain}:{conversation.SessionId}")
                            .SendAsync("ReceiveMessage", conversation.Id.ToString(), message);

                return Result.Success();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while sending message from support to conversation.");
                return Result.Failure(Error.Failure(code: "SendMessageError", description: "An error occurred while sending the message to the conversation."));
            }
        }
    }
}
