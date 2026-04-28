using CCP.Shared.ResultAbstraction;
using ChatService.Sdk.Models;

namespace ChatService.Sdk.Services
{
    public interface IAIReplyClient
    {
        Task<Result<AiReply>> GetReply(int TicketId);
    }
}
