using CCP.Shared.ResultAbstraction;
using Microsoft.Extensions.AI;

namespace ChatService.Application.Interfaces
{
    public interface IEmbeddingService
    {
        Task<Result<Embedding<float>>> GenerateEmbeddingAsync(string input);
    }
}
