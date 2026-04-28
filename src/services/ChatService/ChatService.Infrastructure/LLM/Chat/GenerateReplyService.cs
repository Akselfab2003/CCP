using CCP.Shared.ResultAbstraction;
using ChatService.Domain.Dtos;
using ChatService.Domain.Entities.AI;
using ChatService.Domain.Interfaces;
using ChatService.Infrastructure.LLM.Analysis;
using ChatService.Infrastructure.LLM.Prompts;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChatService.Infrastructure.LLM.Chat
{
    public class GenerateReplyService : IGenerateReplyService
    {
        private readonly IChatClient _chatClient;
        private readonly ILogger<ChatService> _logger;
        private readonly IGeneratedReplyRepository _generatedReplyRepository;

        public GenerateReplyService([FromKeyedServices("qwen")] IChatClient chatClient, ILogger<ChatService> logger, IGeneratedReplyRepository generatedReplyRepository)
        {
            _chatClient = chatClient;
            _logger = logger;
            _generatedReplyRepository = generatedReplyRepository;
        }

        private List<ChatMessage> BuildReplyPrompt(SupportTicket ticket, List<SimilarTicket> similarTickets)
        {
            var messages = ticket.Messages;
            var lastCustomerMessage = messages.OrderByDescending(m => m.SentAt).FirstOrDefault(m => m.AuthorType == MessageAuthorType.User);
            var MessageContent = ticket.Description;
            if (lastCustomerMessage != null)
            {
                MessageContent = lastCustomerMessage.Content;
            }

            return new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System,PromptSections.AIReplyTicketSystemPrompt),
                new ChatMessage(ChatRole.User,
                $"""
                    Here is the full ticket conversation:

                    ===== TICKET =====
                    Title: {ticket.Title}
                    Description: {ticket.Description}

                    Conversation:
                    {TicketFormatter.FormatThread(ticket)}
                    ===== END TICKET =====

                    This is the specific customer message to reply to:
                    ===== LAST CUSTOMER MESSAGE =====
                    {MessageContent}
                    ===== END =====

                    Here are similar resolved cases to base the reply on.
                    Use these as your knowledge — do not invent steps:
                    ===== PAST RESOLVED CASES =====
                    {TicketFormatter.FormatPastSolutions(similarTickets)}
                    ===== END =====

                    Draft a reply the agent can send directly to the customer.
                """)
            };
        }

        public async Task<Result<GeneratedReply>> GenerateReply(SupportTicket ticket, TicketAnalysis analysis, List<SimilarTicket> similarTickets)
        {
            try
            {
                if (similarTickets == null || similarTickets.Count == 0)
                {
                    _logger.LogWarning("No similar tickets found for ticket {TicketId}", ticket.TicketId);
                    return Result.Failure<GeneratedReply>(Error.NotFound("NO_SIMILAR_TICKETS", "No similar tickets found to generate a reply. Please try again later."));
                }

                var messages = BuildReplyPrompt(ticket, similarTickets);

                var response = await _chatClient.GetResponseAsync<AiGeneratedReply>(messages, new ChatOptions()
                {
                    Temperature = 0.3f,
                    ResponseFormat = ChatResponseFormat.Json,
                });

                AiGeneratedReply aiGeneratedReply = response.Result;
                var result = new GeneratedReply
                {
                    Id = Guid.NewGuid(),
                    OrgId = Guid.NewGuid(),
                    TicketId = ticket.TicketId,
                    AgentHeadsUp = aiGeneratedReply.AgentHeadsUp,
                    AlternativeReply = aiGeneratedReply.AlternativeReply,
                    Confidence = aiGeneratedReply.Confidence,
                    InfoNeeded = aiGeneratedReply.InfoNeeded,
                    NeedsMoreInfo = aiGeneratedReply.NeedsMoreInfo,
                    Reasoning = aiGeneratedReply.Reasoning,
                    Reply = aiGeneratedReply.Reply
                };

                await _generatedReplyRepository.AddAsync(result);

                return Result.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating reply with LLM");
                return Result.Failure<GeneratedReply>(Error.Failure("LLM_ERROR", "An error occurred while generating the reply. Please try again later."));
            }
        }



    }

}
