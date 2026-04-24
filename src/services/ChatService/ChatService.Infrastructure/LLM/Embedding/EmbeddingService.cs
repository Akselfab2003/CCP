using CCP.Shared.ResultAbstraction;
using ChatService.Application.Interfaces;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChatService.Infrastructure.LLM.Embedding
{
    public class EmbeddingService : IEmbeddingService
    {
        private readonly ILogger<EmbeddingService> _logger;
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;

        public EmbeddingService(ILogger<EmbeddingService> logger,
                                [FromKeyedServices("embedding")] IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
        {
            _logger = logger;
            _embeddingGenerator = embeddingGenerator;
        }

        public async Task<Result<Embedding<float>>> GenerateEmbeddingAsync(string input)
        {
            try
            {
                var embedding = await _embeddingGenerator.GenerateAsync(input);
                return embedding;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating embedding.");
                throw;
            }
        }
    }
}
