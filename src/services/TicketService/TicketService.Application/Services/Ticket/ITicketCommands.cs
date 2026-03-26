using TicketService.Domain.RequestObjects;

namespace TicketService.Application.Services.Ticket
{
    public interface ITicketCommands
    {
        Task<Result> CreateTicketAsync(CreateTicketRequest request);
    }
}
