using CCP.Shared.ResultAbstraction;

namespace ChatService.Application.Services.Chat
{
    public interface IChatManagementService
    {
        Task<Result<string>> GetChatResponseToMessage(string message, Guid? ConversationId);
    }
}
