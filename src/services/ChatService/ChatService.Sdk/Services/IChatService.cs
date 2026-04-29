using CCP.Shared.ResultAbstraction;

namespace ChatService.Sdk.Services
{
    public interface IChatService
    {
        Task<Result> SendMessageToChatbotTicket(int TicketId, string message);
    }
}
