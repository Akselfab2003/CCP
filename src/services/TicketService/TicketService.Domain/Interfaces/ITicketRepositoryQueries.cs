using ChatApp.Shared.ResultAbstraction;
using ChatApp.Shared.ValueObjects;
using TicketService.Domain.ResponseObjects;

namespace TicketService.Domain.Interfaces
{
    public interface ITicketRepositoryQueries
    {
        Task<Result<TicketDto>> GetTicket(int? id, Guid? AssignmentId = null);
        Task<Result<List<TicketDto>>> GetTicketsBasedOnParameters(Guid? assignedUserId = null, Guid? CustomerId = null, TicketStatus? status = null);
    }
}
