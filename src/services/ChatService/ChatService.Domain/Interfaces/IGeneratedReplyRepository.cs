using CCP.Shared.ResultAbstraction;
using ChatService.Domain.Entities.AI;

namespace ChatService.Domain.Interfaces
{
    public interface IGeneratedReplyRepository
    {
        Task<Result> AddAsync(GeneratedReply generatedReply);
    }
}
