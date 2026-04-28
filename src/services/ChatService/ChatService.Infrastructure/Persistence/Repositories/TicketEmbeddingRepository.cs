using CCP.Shared.ResultAbstraction;
using ChatService.Domain.Dtos;
using ChatService.Domain.Entities.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace ChatService.Infrastructure.Persistence.Repositories
{
    public class TicketEmbeddingRepository : ITicketEmbeddingRepository
    {
        private readonly ILogger<TicketEmbeddingRepository> _logger;
        private readonly ChatDbContext _dbContext;

        public TicketEmbeddingRepository(ILogger<TicketEmbeddingRepository> logger, ChatDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<Result> AddAsync(TicketEmbedding embedding)
        {
            try
            {
                await _dbContext.TicketEmbedding.AddAsync(embedding);
                await _dbContext.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving ticket embedding for ticket {TicketId}", embedding.TicketId);
                return Result.Failure(Error.Failure("DatabaseError", "Failed to save ticket embedding."));
            }
        }

        public async Task<Result<TicketEmbedding>> GetByTicketIdAsync(int ticketId)
        {
            try
            {
                var embedding = await _dbContext.TicketEmbedding.FindAsync(ticketId);
                if (embedding == null)
                    return Result.Failure<TicketEmbedding>(Error.NotFound(code: "EmbeddingNotFound", description: $"No embedding found for ticket {ticketId}."));
                return Result.Success(embedding);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ticket embedding for ticket {TicketId}", ticketId);
                return Result.Failure<TicketEmbedding>(Error.Failure("DatabaseError", "Failed to retrieve ticket embedding."));
            }
        }

        public async Task<Result<List<SimilarTicket>>> SemanticSearch(Vector searchVector, int topK = 5, CancellationToken ct = default)
        {
            try
            {

                var query = _dbContext.TicketEmbedding.Where(e => e.IsSemanticSearchable)
                                                      .Join(_dbContext.TicketAnalysis,
                                                            embedding => embedding.TicketId,
                                                            analysis => analysis.TicketId,
                                                            (embedding, analysis) => new { embedding, analysis });


                return await query.Where(f => f.embedding.ProblemVector != null)
                                  .OrderBy(f => f.embedding.ProblemVector!.L2Distance(searchVector))
                                  .Take(topK)
                                  .Select(f => new SimilarTicket
                                  {
                                      TicketAnalysis = f.analysis,
                                      SimilarityScore = (float)(1 - f.embedding.ProblemVector!.CosineDistance(searchVector))
                                  })
                                  .OrderByDescending(t => t.SimilarityScore)
                                  .ToListAsync(cancellationToken: ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing semantic search for vector {SearchVector}", searchVector);
                return Result.Failure<List<SimilarTicket>>(Error.Failure("DatabaseError", "Failed to perform semantic search."));
            }
        }

        public async Task<Result> UpdateAsync(TicketEmbedding embedding)
        {
            try
            {
                _dbContext.TicketEmbedding.Update(embedding);
                await _dbContext.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ticket embedding for ticket {TicketId}", embedding.TicketId);
                return Result.Failure(Error.Failure("DatabaseError", "Failed to update ticket embedding."));
            }
        }
    }
}
