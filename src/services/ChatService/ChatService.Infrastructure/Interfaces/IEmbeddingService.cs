using System;
using System.Collections.Generic;
using System.Text;

namespace ChatService.Interfaces
{
    public interface IEmbeddingService
    {
        Task<float[]> GetEmbeddingAsync(string text, CancellationToken ct = default);
    }
}
