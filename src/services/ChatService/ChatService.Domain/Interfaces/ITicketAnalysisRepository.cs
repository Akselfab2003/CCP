using CCP.Shared.ResultAbstraction;
using ChatService.Domain.Entities.AI;

namespace ChatService.Domain.Interfaces
{
    public interface ITicketAnalysisRepository
    {
        Task<Result> AddAsync(TicketAnalysis analysis);
        Task<Result<TicketAnalysis>> GetByTicketIdAsync(int ticketId);
        Task<Result> UpdateAsync(TicketAnalysis analysis);
    }
}
