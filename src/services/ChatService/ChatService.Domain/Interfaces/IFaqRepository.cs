using CCP.Shared.ResultAbstraction;
using ChatService.Domain.Entities;
using Pgvector;

namespace ChatService.Domain.Interfaces
{
    public interface IFaqRepository
    {
        Task<Result> AddAsync(FaqEntity faq, CancellationToken ct = default);
        Task<Result<List<FaqEntity>>> GetAllAsync();
        Task<Result<List<FaqEntity>>> SemanticSearch(Vector searchVector, int topK = 5, CancellationToken ct = default);
    }
}
