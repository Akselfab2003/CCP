using CCP.Shared.ResultAbstraction;
using ChatService.Domain.Dtos;

namespace ChatService.Infrastructure.LLM.Analysis
{
    public interface ITicketAnalysisService
    {
        Task<Result<TicketProblemAnalysis>> ExtractProblemAsync(SupportTicket ticket);
        Task<Result<TicketSolutionAnalysis>> ExtractSolutionAsync(SupportTicket ticket);
    }
}
