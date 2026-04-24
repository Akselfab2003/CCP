using CCP.Shared.ResultAbstraction;
using ChatService.Domain.Dtos;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChatService.Infrastructure.LLM.Analysis
{
    public class TicketAnalysisService
    {
        private readonly ILogger<TicketAnalysisService> _logger;
        private readonly IChatClient _chatClient;

        public TicketAnalysisService(ILogger<TicketAnalysisService> logger, [FromKeyedServices("qwen")] IChatClient chatClient)
        {
            _logger = logger;
            _chatClient = chatClient;
        }


        public async Task<Result<string>> ExtractProblemAsync(SupportTicket ticket)
        {
            try
            {
                var thread = TicketFormatter.FormatTicketForAnalysis(ticket);

                var message = new List<ChatMessage>()
                {
                    new ChatMessage(ChatRole.System,"""
                    You are an IT support analyst. Extract structured
                    information from support ticket threads.
                    Respond ONLY in valid JSON — no markdown, no explanation.
                    """),
                    new ChatMessage(ChatRole.User,
                            """
                            Analyze this support ticket thread and extract
                            key problem information. Pay close attention to
                            the full conversation — not just the initial description.
                            """+
                            thread
                            +
                            """
                            Respond in this exact JSON format:
                            {{
                              "summary": "one sentence problem summary based on full thread",
                              "category": "auth|network|hardware|software|billing|other",
                              "component": "which system or service is affected",
                              "symptoms": ["symptom1", "symptom2"],
                              "errorCodes": ["any error codes mentioned in thread"],
                              "tags": ["relevant", "keywords"],
                              "clarifications": ["extra details found in messages not in description"]
                            }}
                            """)
                };

                var response = await _chatClient.GetResponseAsync<TicketProblemAnalysis>(message);


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting problem from ticket {TicketId}", ticket.TicketId);
                return Result.Failure<string>(Error.Failure(code: "TicketProblemExtractionError", description: $"An error occurred while extracting the problem from ticket {ticket.TicketId}."));
            }
        }
    }
}
