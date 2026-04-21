using CCP.Shared.ResultAbstraction;
using ChatService.Domain.Entities;

namespace ChatService.Domain.Interfaces
{
    public interface IMessageRepository
    {
        Task<Result> AddMessage(MessageEntity message);
        Task<Result<List<MessageEntity>>> GetMessagesByConversationId(Guid conversationId);
    }
}
