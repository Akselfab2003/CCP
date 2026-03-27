using System;
using System.Collections.Generic;
using System.Text;
using ChatService.Models;

namespace ChatService.Interfaces
{
    public interface IChatService
    {
        Task<ChatResponse> HandleAsync(
            ChatRequest request, string bearerToken, CancellationToken ct = default);
    }
}
