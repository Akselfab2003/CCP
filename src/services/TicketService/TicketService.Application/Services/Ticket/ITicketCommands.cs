using TicketService.Domain.RequestObjects;

namespace TicketService.Application.Services.Ticket
{
    public interface ITicketCommands
    {
        Task<Result<int>> CreateTicketAsync(CreateTicketRequest request);
        Task<Result> UpdateTicketStatusAsync(int ticketId, TicketStatus newStatus);
        Task<Result> RecordMessageSentAsync(int ticketId, Guid? senderUserId, string messageSnippet);
    }
}
