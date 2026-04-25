using ChatService.Domain.Dtos;
using ChatService.Domain.Entities.AI;
using ChatService.Infrastructure.LLM.Analysis;
using ChatService.Infrastructure.Persistence;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChatService.Infrastructure.LLM.Embedding
{
    public class TicketEmbeddingOrchestrator
    {
        private readonly ILogger<TicketEmbeddingOrchestrator> _logger;
        private readonly ITicketAnalysisService _ticketAnalysisService;
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;

        public TicketEmbeddingOrchestrator(ILogger<TicketEmbeddingOrchestrator> logger,
                                           ITicketAnalysisService ticketAnalysisService,
                                           ChatDbContext chatDbContext,
                                           [FromKeyedServices("qwen")] IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
        {
            _logger = logger;
            _ticketAnalysisService = ticketAnalysisService;
            _chatDbContext = chatDbContext;
            _embeddingGenerator = embeddingGenerator;
        }

        public async Task OnTicketCreatedAsync(SupportTicket ticket)
        {
            try
            {
                var analysisResult = await _ticketAnalysisService.ExtractProblemAsync(ticket);

                if (analysisResult.IsFailure)
                {
                    _logger.LogWarning("Problem extraction failed for ticket {TicketId}: {ErrorMessage}", ticket.TicketId, analysisResult.ErrorMessage);
                    return;
                }
                var analysisData = analysisResult.Value;

                var entity = new TicketAnalysis()
                {
                    TicketId = ticket.TicketId,
                    OrgId = ticket.OrgId,
                    ProblemSummary = analysisData.Summary,
                    Category = analysisData.Category,
                    Component = analysisData.Component,
                    Symptoms = analysisData.Symptoms,
                    ErrorCodes = analysisData.ErrorCodes,
                    Tags = analysisData.Tags,
                    ProblemAnalysedAt = DateTime.UtcNow
                };


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating embedding for ticket {TicketId}", ticket.TicketId);
            }
        }

        public async Task OnNewMessageAsync(SupportTicket ticket)
        {
        }

        public async Task OnTicketClosedAsync(SupportTicket ticket)
        {
        }
    }
}
