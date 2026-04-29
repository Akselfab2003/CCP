using CCP.Shared.ResultAbstraction;
using ChatService.Domain.Entities;

namespace ChatService.Domain.Interfaces
{
    public interface IConversationRepository
    {
        Task<Result> AddConversation(ConversationEntity conversation);
        Task<Result<ConversationEntity>> GetConversationById(Guid conversationId);
        Task<Result<List<ConversationEntity>>> GetConversationsBySessionId(Guid SessionId);
        Task<Result<ConversationEntity>> GetConversationsByTicketId(int ticketId);
        Task<Result> UpdateConversation(ConversationEntity conversation);
    }
}
