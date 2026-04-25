using CCP.Shared.ResultAbstraction;
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

        public async Task<Result<List<TicketEmbedding>>> SemanticSearch(Vector searchVector, int topK = 5, CancellationToken ct = default)
        {
            try
            {
                var faqs = await _dbContext.TicketEmbedding.Where(f => f.ProblemVector != null)
                                                    .OrderBy(f => f.ProblemVector!.L2Distance(searchVector))
                                                    .Take(topK)
                                                    .ToListAsync(cancellationToken: ct);
                return Result.Success(faqs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not perform semantic search with vector");
                return Result.Failure<List<TicketEmbedding>>(Error.Failure("TicketEmbeddingSearchFailed", "An error occurred while performing semantic search for ticket embeddings."));
            }
        }
    }
}
