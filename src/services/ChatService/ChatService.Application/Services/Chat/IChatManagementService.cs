using CCP.Shared.ResultAbstraction;

namespace ChatService.Application.Services.Chat
{
    public interface IChatManagementService
    {
        Task<Result<Guid>> CreateConversation(Guid SessionId);
        Task<Result<string>> GetChatResponseToMessage(string message, Guid? ConversationId);
        Task<Result> SendMessageFromSupportToConversation(int ticketId, string message);
    }
}
