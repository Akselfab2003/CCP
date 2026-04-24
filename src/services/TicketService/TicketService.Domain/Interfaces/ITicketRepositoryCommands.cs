using TicketService.Domain.Entities;

namespace TicketService.Domain.Interfaces
{
    public interface ITicketRepositoryCommands
    {
        Task<Result<Ticket>> AddAsync(Ticket ticket);
        Task<Result<Ticket>> GetTicket(int? id, Guid? AssignmentId = null);
        Task<Result<List<Ticket>>> GetTicketsBasedOnParameters(Guid? assignedUserId = null, Guid? CustomerId = null, TicketStatus? status = null);
        Task SaveChangesAsync();
        Task<Result> UpdateStatusAsync(int ticketId, TicketStatus newStatus);
    }
}
