using CCP.Shared.ResultAbstraction;
using ChatService.Domain.Dtos;
using ChatService.Domain.Entities.AI;

namespace ChatService.Infrastructure.LLM.Chat
{
    public interface IGenerateReplyService
    {
        Task<Result<GeneratedReply>> GenerateReply(SupportTicket ticket, TicketAnalysis analysis, List<SimilarTicket> similarTickets);
    }
}