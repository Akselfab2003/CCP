using CCP.Shared.ResultAbstraction;
using ChatService.Domain.Dtos;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChatService.Infrastructure.LLM.Analysis
{
    public class TicketAnalysisService : ITicketAnalysisService
    {
        private readonly ILogger<TicketAnalysisService> _logger;
        private readonly IChatClient _chatClient;

        public TicketAnalysisService(ILogger<TicketAnalysisService> logger, [FromKeyedServices("qwen")] IChatClient chatClient)
        {
            _logger = logger;
            _chatClient = chatClient;
        }


        public async Task<Result<TicketProblemAnalysis>> ExtractProblemAsync(SupportTicket ticket)
        {
            try
            {
                var thread = TicketFormatter.FormatTicketForAnalysis(ticket);

                var message = new List<ChatMessage>()
                {
                    new ChatMessage(ChatRole.System,"""
                    You are an IT support analyst. Your job is to extract
                    structured information from support ticket threads.

                    RULES:
                    - Base your analysis ONLY on the ticket content provided
                    - Never assume or invent details not present in the thread
                    - If a field has no clear value, use an empty array or null
                    - Respond ONLY in valid JSON — no markdown, no explanation
                    """),
                    new ChatMessage(ChatRole.User,
                            """
                            Analyze this support ticket and extract the key problem
                            information. Pay attention to the full thread, not just
                            the initial description — customers often add important
                            details in follow up messages.
                            """+
                            thread
                            +
                            """
                            Respond in this exact JSON format:
                            {{
                                "summary": "one clear sentence describing the problem",
                                "category": "auth|network|hardware|software|billing|other",
                                "component": "the specific system or service affected",
                                "symptoms": ["symptom1", "symptom2"],
                                "errorCodes": ["any error codes or HTTP codes mentioned"],
                                "tags": ["relevant", "searchable", "keywords"],
                                "clarifications": [
                                "extra details found in messages not in the description"
                                ]
                            }}
                            """)
                };

                var response = await _chatClient.GetResponseAsync<TicketProblemAnalysis>(message, GetChatOptions());


                if (response == null)
                    return Result.Failure<TicketProblemAnalysis>(Error.NotFound(code: "TicketProblemNotFound", description: $"Could not extract problem information from ticket {ticket.TicketId}."));

                return response.Result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting problem from ticket {TicketId}", ticket.TicketId);
                return Result.Failure<TicketProblemAnalysis>(Error.Failure(code: "TicketProblemExtractionError", description: $"An error occurred while extracting the problem from ticket {ticket.TicketId}."));
            }
        }
        public async Task<Result<TicketSolutionAnalysis>> ExtractSolutionAsync(SupportTicket ticket)
        {
            try
            {
                var thread = TicketFormatter.FormatTicketForAnalysis(ticket);

                var message = new List<ChatMessage>()
                {
                    new ChatMessage(ChatRole.System,"""
                    You are an IT support analyst. Your job is to extract
                    the solution from a resolved support ticket thread.

                    RULES:
                    - The solution is found in agent messages and resolution notes
                    - Focus on what actually fixed the issue, not what was tried
                        and failed
                    - Never assume or invent steps not present in the thread
                    - If a field has no clear value, use an empty array or null
                    - Respond ONLY in valid JSON — no markdown, no explanation
                    """),
                    new ChatMessage(ChatRole.User,
                            """
                            This ticket has been resolved. Analyze the full thread
                            and extract what fixed the issue.
                            """+
                            thread
                            +
                            """
                            Respond in this exact JSON format:
                            {{
                                "rootCause": "what actually caused the issue",
                                "solutionSummary": "one sentence describing the fix",
                                "solutionSteps": [
                                "exact step 1 the agent took",
                                "exact step 2"
                                ],
                                "diagnosticSteps": [
                                "things that were checked but ruled out"
                                ],
                                "preventionTips": [
                                "how to avoid this issue in future"
                                ]
                            }}
                            """)
                };

                var response = await _chatClient.GetResponseAsync<TicketSolutionAnalysis>(message, GetChatOptions());


                if (response == null)
                    return Result.Failure<TicketSolutionAnalysis>(Error.NotFound(code: "TicketSolutionNotFound", description: $"Could not extract solution information from ticket {ticket.TicketId}."));

                return response.Result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting solution from ticket {TicketId}", ticket.TicketId);
                return Result.Failure<TicketSolutionAnalysis>(Error.Failure(code: "TicketSolutionExtractionError", description: $"An error occurred while extracting the solution from ticket {ticket.TicketId}."));
            }
        }

        private ChatOptions GetChatOptions()
        {
            return new ChatOptions()
            {
                Temperature = 0.1f,
                ResponseFormat = ChatResponseFormat.Json,
            };
        }
    }
}
