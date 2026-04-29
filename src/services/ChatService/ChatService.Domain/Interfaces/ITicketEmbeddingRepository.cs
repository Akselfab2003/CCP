using CCP.Shared.ResultAbstraction;
using ChatService.Domain.Dtos;
using ChatService.Domain.Entities.AI;
using Pgvector;

namespace ChatService.Infrastructure.Persistence.Repositories
{
    public interface ITicketEmbeddingRepository
    {
        Task<Result> AddAsync(TicketEmbedding embedding);
        Task<Result<TicketEmbedding>> GetByTicketIdAsync(int ticketId);
        Task<Result<List<SimilarTicket>>> SemanticSearch(Vector searchVector, int topK = 5, CancellationToken ct = default);
        Task<Result> UpdateAsync(TicketEmbedding embedding);
    }
}
