using CCP.Shared.ResultAbstraction;
using ChatService.Domain.Entities.AI;

namespace ChatService.Application.Services.Automated
{
    public interface IAutomaticMessageGeneration
    {
        Task<Result<GeneratedReply>> GenerateReplyUsingAI(int ticketId);
        Task<Result> NewMessageAddedToTicketAnalysis(int ticketId);
        Task<Result> TicketClosedAnalysis(int ticketId);
        Task<Result> TicketCreatedAnalysis(int ticketId);
    }
}
