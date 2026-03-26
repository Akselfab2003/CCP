using TicketService.Domain.ResponseObjects;

namespace TicketService.Application.Services.Ticket
{
    public interface ITicketQueries
    {
        Task<Result<TicketDto>> GetTicket(int ticketId);
        Task<Result<List<TicketDto>>> GetTicketsBasedOnParameters(Guid? assignedUserId = null, Guid? CustomerId = null, TicketStatus? status = null);
    }
}
