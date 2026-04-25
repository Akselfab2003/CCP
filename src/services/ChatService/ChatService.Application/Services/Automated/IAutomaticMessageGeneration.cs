using CCP.Shared.ResultAbstraction;

namespace ChatService.Application.Services.Automated
{
    public interface IAutomaticMessageGeneration
    {
        Task<Result> NewMessageAddedToTicketAnalysis(int ticketId);
        Task<Result> TicketClosedAnalysis(int ticketId);
        Task<Result> TicketCreatedAnalysis(int ticketId);
    }
}
