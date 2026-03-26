using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using ChatService.Interfaces;
using ChatService.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChatService.Repositories
{
    public class EmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _http;
        private readonly string _model;
        private readonly ILogger<EmbeddingService> _logger;

        public EmbeddingService(HttpClient http, IOptions<ChatOptions> opts,
            ILogger<EmbeddingService> logger)
        {
            _http = http;
            _model = opts.Value.EmbeddingModel;
            _logger = logger;
        }

        public async Task<float[]> GetEmbeddingAsync(string text, CancellationToken ct = default)
        {
            try
            {
                var payload = new { model = _model, input = text };
                var response = await _http.PostAsJsonAsync("/api/embed", payload, ct);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content
                        .ReadFromJsonAsync<OllamaEmbedResponse>(cancellationToken: ct);

                    _logger.LogInformation("Embed response: {Count} embeddings, første længde: {Len}",
                        result?.Embeddings?.Count ?? 0,
                        result?.Embeddings?.FirstOrDefault()?.Length ?? 0);

                    var embedding = result?.Embeddings?.FirstOrDefault();
                    if (embedding != null && embedding.Length > 0)
                        return embedding;
                }

                _logger.LogWarning("Embed endpoint fejlede med {Status} — prøver /api/embeddings",
                    response.StatusCode);

                // Fallback til gammelt endpoint
                var oldPayload = new { model = _model, prompt = text };
                var oldResponse = await _http.PostAsJsonAsync("/api/embeddings", oldPayload, ct);
                oldResponse.EnsureSuccessStatusCode();

                var oldResult = await oldResponse.Content
                    .ReadFromJsonAsync<OllamaEmbeddingResponse>(cancellationToken: ct);

                _logger.LogInformation("Gammelt embed response længde: {Len}",
                    oldResult?.Embedding?.Length ?? 0);

                return oldResult?.Embedding ?? [];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Embedding fejlede for model {Model}", _model);
                return [];
            }
        }

        private record OllamaEmbedResponse(List<float[]> Embeddings);
        private record OllamaEmbeddingResponse(float[] Embedding);

    }
}
