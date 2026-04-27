using CCP.Shared.AuthContext;
using CCP.Shared.ResultAbstraction;
using ChatService.Application.Interfaces;
using ChatService.Domain.Dtos;
using ChatService.Domain.Entities;
using ChatService.Domain.Interfaces;
using ChatService.Infrastructure.LLM.Prompts;
using MessagingService.Sdk.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TicketService.Sdk.Services.Ticket;
namespace ChatService.Infrastructure.LLM.Chat
{
    /// <summary>
    /// Service responsible for handling chat interactions with the chatbot.
    /// </summary>
    public class ChatService : IChatService
    {
        private readonly IChatClient _chatClient;
        private readonly ILogger<ChatService> _logger;
        private readonly IConversationRepository _conversationRepository;
        private readonly ITicketService _ticketService;
        private readonly ServiceAccountOverrider _serviceAccountOverrider;
        private readonly IMessageSdkService _messageSdkService;
        public ChatService([FromKeyedServices("qwen")] IChatClient chatClient,
                           ILogger<ChatService> logger,
                           IConversationRepository conversationRepository,
                           ITicketService ticketService,
                           ServiceAccountOverrider serviceAccountOverrider,
                           IMessageSdkService messageSdkService)
        {
            _chatClient = chatClient;
            _logger = logger;
            _conversationRepository = conversationRepository;
            _ticketService = ticketService;
            _serviceAccountOverrider = serviceAccountOverrider;
            _messageSdkService = messageSdkService;
        }

        public async Task<Result<MessageEntity>> GetChatBotResponseAsync(string userMessage, List<FaqEntity> mostRelevantFaqs, List<MessageEntity> History, Guid ConversationId, Guid OrgId, string OrgName)
        {
            try
            {

                var routingResult = await RouteAsync(userMessage, History);

                if (routingResult.IsSuccess)
                {
                    if (routingResult.Value.ShouldEscalate)
                    {
                        _logger.LogInformation("Message routed to human agent.");
                        var messageEntity = new MessageEntity
                        {
                            Id = Guid.NewGuid(),
                            OrgId = OrgId,
                            ConversationId = ConversationId,
                            Message = "A supporter is on their way to assist you.",
                            IsFromUser = false,
                            CreatedAt = DateTime.UtcNow
                        };
                        History.Add(messageEntity);
                        await EscalateToHumanAgent(routingResult.Value, History, OrgId);
                        return Result.Success(messageEntity);
                    }
                    else
                    {
                        _logger.LogInformation("Message will be handled by chatbot.");
                        var faqReplyResult = await ChatBotFaqReply(userMessage, mostRelevantFaqs, History, ConversationId, OrgId, OrgName);
                        return faqReplyResult;
                    }

                }
                else
                {
                    _logger.LogWarning("Routing decision failed, defaulting to chatbot response. Error: {Error}", routingResult.Error);
                    var faqReplyResult = await ChatBotFaqReply(userMessage, mostRelevantFaqs, History, ConversationId, OrgId, OrgName);
                    return faqReplyResult;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting chat response.");
                return Result.Failure<MessageEntity>(Error.Failure("ChatServiceError", "An error occurred while processing the chat response."));
            }
        }

        private async Task<Result<RouterDecision>> RouteAsync(string userMessage, List<MessageEntity> History)
        {
            try
            {
                var messages = new List<ChatMessage>
                {
                    new(ChatRole.System, """
                        You are a message router for a customer support chatbot.
                        Your ONLY job is to decide if a message should be
                        escalated to a human agent or answered by the chatbot.

                        Escalate when ANY of these are true:
                        - User explicitly asks for a human or live agent
                        - User mentions payment, billing, charges or refunds
                        - User mentions account security or unauthorized access
                        - User mentions legal matters or complaints
                        - User is clearly frustrated or angry
                        - User is repeating a question they already asked

                        Do NOT escalate when:
                        - User is asking a general question
                        - User is greeting the chatbot
                        - User is asking something the FAQ might cover
                        - You are unsure — default to NOT escalating

                        Respond ONLY with a RouterDecision JSON object.
                        {
                            "ShouldEscalate": bool,
                            "Reason": string (one sentence),
                            "Summary": string (summary of the user message, 5 words or less)
                        }
                        """),


                    new(ChatRole.User, $"""
                        Conversation so far:
                        {FormatRecentHistory(History, lastN: 4)}

                        New message from customer:
                        {userMessage}

                        Should this be escalated to a human agent?
                        """)
                };

                ChatResponse<RouterDecision> response = await _chatClient.GetResponseAsync<RouterDecision>(messages: messages, new ChatOptions()
                {
                    Temperature = 0f,
                    ResponseFormat = ChatResponseFormat.Json
                });

                return Result.Success(response.Result);
            }
            catch (Exception)
            {
                return Result.Failure<RouterDecision>(Error.Failure("RoutingError", "An error occurred while routing the message."));
            }
        }

        private async Task<Result<MessageEntity>> ChatBotFaqReply(string userMessage, List<FaqEntity> mostRelevantFaqs, List<MessageEntity> History, Guid ConversationId, Guid OrgId, string OrgName)
        {
            try
            {
                var FullPrompt = ChatPrompts.Build(UserMessage: userMessage,
                                                     CompanyName: OrgName,
                                                     faqEntities: mostRelevantFaqs,
                                                     history: History);


                var options = new ChatOptions
                {
                    Temperature = 0.7f,
                    MaxOutputTokens = 1000,
                    Instructions = FullPrompt,
                    ResponseFormat = ChatResponseFormat.Text
                };


                var HistoryMessages = ChatPrompts.BuildHistory(History);
                HistoryMessages.Add(new ChatMessage(ChatRole.User, userMessage));

                var response = await _chatClient.GetResponseAsync(HistoryMessages, options);
                // Create a MessageEntity for the response
                var messageEntity = new MessageEntity
                {
                    Id = Guid.NewGuid(),
                    OrgId = OrgId,
                    ConversationId = ConversationId,
                    Message = response.Text,
                    IsFromUser = false,
                    CreatedAt = DateTime.UtcNow
                };

                return Result.Success(messageEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting chatbot FAQ response.");
                return Result.Failure<MessageEntity>(Error.Failure("ChatBotFaqReplyError", "An error occurred while getting the chatbot FAQ response."));
            }
        }

        private string FormatRecentHistory(List<MessageEntity> history, int lastN)
        {
            var recentMessages = history.OrderByDescending(h => h.CreatedAt).Take(lastN).Reverse();
            return string.Join("\n", recentMessages.Select(m =>
                $"{(m.IsFromUser ? "USER" : "Aria")}: {m.Message}"));
        }

        private async Task<Result> EscalateToHumanAgent(RouterDecision decision, List<MessageEntity> messages, Guid TenantId)
        {
            try
            {

                var newestMessage = messages.OrderBy(m => m.CreatedAt).First();



                _serviceAccountOverrider.SetOrganizationId(TenantId);

                var createTicketResult = await _ticketService.CreateTicket(new TicketService.Sdk.Dtos.CreateTicketRequestDto()
                {
                    Title = $"Support request from customer: {decision.Summary}",
                    OrganizationId = TenantId,
                    Description = $"The following message was escalated to a human agent:\n\n{newestMessage.Message}\n\nReason for escalation: {decision.Reason}",
                }, CCP.Shared.ValueObjects.TicketOrigin.Chatbot);

                if (createTicketResult.IsSuccess)
                {
                    var conversation = await _conversationRepository.GetConversationById(newestMessage.ConversationId);
                    if (conversation == null || conversation.IsFailure) return Result.Failure(Error.NotFound("ConversationNotFound", "Conversation not found for escalation."));

                    ConversationEntity conversationEntity = conversation.Value;
                    var ticketId = createTicketResult.Value;
                    conversationEntity.IsEscalated = true;
                    conversationEntity.EscalatedTicketId = ticketId;
                    await _conversationRepository.UpdateConversation(conversationEntity);


                    foreach (var msg in messages)
                    {
                        await _messageSdkService.CreateMessageAsync(ticketId, TenantId, null, msg.Message, false);
                    }

                    return Result.Success(createTicketResult);

                }
                else
                {
                    _logger.LogError("Failed to create ticket for escalation. Error: {Error}", createTicketResult.Error);
                    return Result.Failure(Error.Failure("TicketCreationError", "Failed to create a ticket for escalation."));
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while escalating to human agent.");
                return Result.Failure(Error.Failure("EscalationError", "An error occurred while escalating to a human agent."));
            }
        }
    }
}
