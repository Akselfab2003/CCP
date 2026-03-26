using ChatApp.Shared.ResultAbstraction;
using TicketService.Domain.Entities;

namespace TicketService.Domain.Interfaces
{
    public interface IAssignmentRepository
    {
        Task<Result<Assignment>> AddAsync(Assignment assignment);
        Task<Result<Assignment>> GetAssignmentByTicketIdAsync(int ticketId);
        Task<Result<Assignment>> GetByIdAsync(Guid id);
        Task<Result<Assignment>> UpdateAsync(Assignment assignment);
        Task SaveChangesAsync();
    }
}
