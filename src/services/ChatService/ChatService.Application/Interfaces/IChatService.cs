using CCP.Shared.ResultAbstraction;

namespace ChatService.Application.Interfaces
{
    public interface IChatService
    {
        Task<Result<string>> GetChatResponseAsync(string userMessage);
    }
}
