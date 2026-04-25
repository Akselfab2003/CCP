using CCP.Shared.ResultAbstraction;

namespace ChatService.Infrastructure.LLM.Analysis
{
    public interface ITicketAnalysisService
    {
        Task<Result<TicketProblemAnalysis>> ExtractProblemAsync(SupportTicket ticket);
    }
}
