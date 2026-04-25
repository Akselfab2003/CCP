using ChatService.Domain.Dtos;
using ChatService.Domain.Entities.AI;
using ChatService.Infrastructure.LLM.Analysis;
using ChatService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChatService.Infrastructure.LLM.Embedding
{
    public class TicketEmbeddingOrchestrator : ITicketEmbeddingOrchestrator
    {
        private readonly ILogger<TicketEmbeddingOrchestrator> _logger;
        private readonly ITicketAnalysisService _ticketAnalysisService;
        private readonly ChatDbContext _chatDbContext;
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;

        private readonly EmbeddingTextBuilder _embeddingTextBuilder;
        public TicketEmbeddingOrchestrator(ILogger<TicketEmbeddingOrchestrator> logger,
                                           ITicketAnalysisService ticketAnalysisService,
                                           [FromKeyedServices("qwen")] IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
                                           ChatDbContext chatDbContext)
        {
            _logger = logger;
            _ticketAnalysisService = ticketAnalysisService;
            _embeddingGenerator = embeddingGenerator;
            _chatDbContext = chatDbContext;
            _embeddingTextBuilder = new EmbeddingTextBuilder();
        }

        public async Task OnTicketCreatedAsync(SupportTicket ticket)
        {
            try
            {
                var analysisResult = await _ticketAnalysisService.ExtractProblemAsync(ticket);

                if (analysisResult.IsFailure)
                {
                    _logger.LogWarning("Problem extraction failed for ticket {TicketId}: {ErrorMessage}", ticket.TicketId, analysisResult.Error.Description);
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
                    ProblemAnalysedAt = DateTime.UtcNow,
                };

                _chatDbContext.TicketAnalysis.Add(entity);
                await _chatDbContext.SaveChangesAsync();

                await EmbedProblemAsync(entity, analysisData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating embedding for ticket {TicketId}", ticket.TicketId);
            }
        }

        public async Task OnNewMessageAsync(SupportTicket ticket)
        {
            try
            {
                var existing = await _chatDbContext.TicketAnalysis.Include(a => a.Embedding).FirstOrDefaultAsync(a => a.TicketId == ticket.TicketId);

                if (existing is null)
                {
                    await OnTicketCreatedAsync(ticket);
                    return;
                }

                var analysisResult = await _ticketAnalysisService.ExtractProblemAsync(ticket);

                if (analysisResult.IsFailure)
                {
                    _logger.LogWarning("Problem extraction failed for ticket {TicketId}: {ErrorMessage}", ticket.TicketId, analysisResult.Error.Description);
                    return;
                }

                if (analysisResult.Value.Summary == existing.ProblemSummary)
                    return;

                existing.ProblemSummary = analysisResult.Value.Summary;
                existing.Category = analysisResult.Value.Category;
                existing.Component = analysisResult.Value.Component;
                existing.Symptoms = analysisResult.Value.Symptoms;
                existing.ErrorCodes = analysisResult.Value.ErrorCodes;
                existing.Tags = analysisResult.Value.Tags;
                existing.ProblemAnalysedAt = DateTime.UtcNow;
                existing.ReanalysisCount += 1;
                await _chatDbContext.SaveChangesAsync();
                await EmbedProblemAsync(existing, analysisResult.Value);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating embedding for ticket {TicketId}", ticket.TicketId);
            }
        }

        public async Task OnTicketClosedAsync(SupportTicket ticket)
        {
            try
            {
                var existing = await _chatDbContext.TicketAnalysis
                                                   .Include(a => a.Embedding)
                                                   .FirstOrDefaultAsync(a => a.TicketId == ticket.TicketId);

                if (existing is null)
                {
                    // This covers the edge case where a ticket is created and closed before the OnTicketCreatedAsync is processed. In such cases, we can directly create the analysis and embedding for the closed ticket before generating the solution embedding.
                    await OnTicketCreatedAsync(ticket);
                    existing = await _chatDbContext.TicketAnalysis
                                                   .Include(a => a.Embedding)
                                                   .FirstAsync(a => a.TicketId == ticket.TicketId);
                }

                var solutionAnalysisResult = await _ticketAnalysisService.ExtractSolutionAsync(ticket);
                if (solutionAnalysisResult.IsFailure)
                {
                    _logger.LogError("Solution extraction failed for ticket {TicketId}: {ErrorMessage}", ticket.TicketId, solutionAnalysisResult.Error.Description);
                    return;
                }

                existing.RootCause = solutionAnalysisResult.Value.RootCause;
                existing.SolutionSummary = solutionAnalysisResult.Value.SolutionSummary;
                existing.SolutionSteps = solutionAnalysisResult.Value.SolutionSteps;
                existing.PreventionTips = solutionAnalysisResult.Value.PreventionTips;
                existing.SolutionAnalysedAt = DateTime.UtcNow;

                await _chatDbContext.SaveChangesAsync();

                await EmbedSolutionAsync(existing, solutionAnalysisResult.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing ticket {TicketId}", ticket.TicketId);
            }
        }




        private async Task EmbedProblemAsync(TicketAnalysis analysis, TicketProblemAnalysis problemAnalysis)
        {
            var text = _embeddingTextBuilder.BuildProblemText(problemAnalysis);
            var embeddingResult = await _embeddingGenerator.GenerateAsync(text);
            Pgvector.Vector vector = new Pgvector.Vector(embeddingResult.Vector);

            var embedding = analysis.Embedding ?? new TicketEmbedding()
            {
                TicketId = analysis.TicketId,
                OrgId = analysis.OrgId,
            };

            embedding.ProblemEmbeddingSource = text;
            embedding.ProblemVector = vector;
            embedding.ProblemEmbeddedAt = DateTime.UtcNow;
            embedding.IsSemanticSearchable = false;

            if (analysis.Embedding is null)
                await _chatDbContext.TicketEmbedding.AddAsync(embedding);

            await _chatDbContext.SaveChangesAsync();

        }


        private async Task EmbedSolutionAsync(TicketAnalysis analysis, TicketSolutionAnalysis solution)
        {
            var text = _embeddingTextBuilder.BuildSolutionText(analysis, solution);
            var embeddingResult = await _embeddingGenerator.GenerateAsync(text);
            Pgvector.Vector vector = new Pgvector.Vector(embeddingResult.Vector);

            var embedding = analysis.Embedding!;

            embedding.SolutionEmbeddingSource = text;
            embedding.SolutionVector = vector;
            embedding.SolutionEmbeddedAt = DateTime.UtcNow;
            embedding.IsSemanticSearchable = true;

            await _chatDbContext.SaveChangesAsync();
        }
    }
}
