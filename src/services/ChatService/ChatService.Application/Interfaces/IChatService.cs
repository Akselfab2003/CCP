using CCP.Shared.ResultAbstraction;
using ChatService.Domain.Entities;

namespace ChatService.Application.Interfaces
{
    public interface IChatService
    {
        Task<Result<MessageEntity>> GetChatBotResponseAsync(string userMessage, List<FaqEntity> mostRelevantFaqs, List<MessageEntity> History, Guid ConversationId, Guid OrgId, string OrgName);
    }
}
