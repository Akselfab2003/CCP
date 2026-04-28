using CCP.Shared.ResultAbstraction;
using ChatService.Domain.Dtos;
using ChatService.Domain.Entities.AI;
using ChatService.Domain.Interfaces;
using ChatService.Infrastructure.LLM.Analysis;
using ChatService.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChatService.Infrastructure.LLM.Embedding
{
    public class TicketEmbeddingOrchestrator : ITicketEmbeddingOrchestrator
    {
        private readonly ILogger<TicketEmbeddingOrchestrator> _logger;
        private readonly ITicketAnalysisService _ticketAnalysisService;
        private readonly ITicketAnalysisRepository _ticketAnalysisRepository;
        private readonly ITicketEmbeddingRepository _ticketEmbeddingRepository;
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;

        private readonly EmbeddingTextBuilder _embeddingTextBuilder;
        public TicketEmbeddingOrchestrator(ILogger<TicketEmbeddingOrchestrator> logger,
                                           ITicketAnalysisService ticketAnalysisService,
                                           [FromKeyedServices("embedding")] IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
                                           ITicketAnalysisRepository ticketAnalysisRepository,
                                           ITicketEmbeddingRepository ticketEmbeddingRepository)
        {
            _logger = logger;
            _ticketAnalysisService = ticketAnalysisService;
            _embeddingGenerator = embeddingGenerator;
            _embeddingTextBuilder = new EmbeddingTextBuilder();
            _ticketAnalysisRepository = ticketAnalysisRepository;
            _ticketEmbeddingRepository = ticketEmbeddingRepository;
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

                await _ticketAnalysisRepository.AddAsync(entity);

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
                var existing = await _ticketAnalysisRepository.GetByTicketIdAsync(ticket.TicketId);

                if (existing is null)
                {
                    await OnTicketCreatedAsync(ticket);
                    return;
                }
                var ticketAnalysis = existing.Value;

                var analysisResult = await _ticketAnalysisService.ExtractProblemAsync(ticket);

                if (analysisResult.IsFailure)
                {
                    _logger.LogWarning("Problem extraction failed for ticket {TicketId}: {ErrorMessage}", ticket.TicketId, analysisResult.Error.Description);
                    return;
                }

                if (analysisResult.Value.Summary == ticketAnalysis.ProblemSummary)
                    return;

                ticketAnalysis.ProblemSummary = analysisResult.Value.Summary;
                ticketAnalysis.Category = analysisResult.Value.Category;
                ticketAnalysis.Component = analysisResult.Value.Component;
                ticketAnalysis.Symptoms = analysisResult.Value.Symptoms;
                ticketAnalysis.ErrorCodes = analysisResult.Value.ErrorCodes;
                ticketAnalysis.Tags = analysisResult.Value.Tags;
                ticketAnalysis.ProblemAnalysedAt = DateTime.UtcNow;
                ticketAnalysis.ReanalysisCount += 1;
                Result result = await _ticketAnalysisRepository.UpdateAsync(ticketAnalysis);
                await EmbedProblemAsync(ticketAnalysis, analysisResult.Value);

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
                var existing = await _ticketAnalysisRepository.GetByTicketIdAsync(ticket.TicketId);



                if (existing is null)
                {
                    // This covers the edge case where a ticket is created and closed before the OnTicketCreatedAsync is processed. In such cases, we can directly create the analysis and embedding for the closed ticket before generating the solution embedding.
                    await OnTicketCreatedAsync(ticket);
                    existing = await _ticketAnalysisRepository.GetByTicketIdAsync(ticket.TicketId);
                }
                var ticketAnalysis = existing.Value;

                var solutionAnalysisResult = await _ticketAnalysisService.ExtractSolutionAsync(ticket);
                if (solutionAnalysisResult.IsFailure)
                {
                    _logger.LogError("Solution extraction failed for ticket {TicketId}: {ErrorMessage}", ticket.TicketId, solutionAnalysisResult.Error.Description);
                    return;
                }

                ticketAnalysis.RootCause = solutionAnalysisResult.Value.RootCause;
                ticketAnalysis.SolutionSummary = solutionAnalysisResult.Value.SolutionSummary;
                ticketAnalysis.SolutionSteps = solutionAnalysisResult.Value.SolutionSteps;
                ticketAnalysis.PreventionTips = solutionAnalysisResult.Value.PreventionTips;
                ticketAnalysis.SolutionAnalysedAt = DateTime.UtcNow;

                await _ticketAnalysisRepository.UpdateAsync(ticketAnalysis);

                await EmbedSolutionAsync(ticketAnalysis, solutionAnalysisResult.Value);
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
                await _ticketEmbeddingRepository.AddAsync(embedding);
            else
                await _ticketEmbeddingRepository.UpdateAsync(embedding);
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

            await _ticketEmbeddingRepository.UpdateAsync(embedding);
        }
    }
}
