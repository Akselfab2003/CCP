using TicketService.Domain.Entities;

namespace TicketService.Domain.Interfaces
{
    public interface ITicketHistoryRepository
    {
        Task AddAsync(TicketHistoryEntry entry, CancellationToken ct = default);
        Task<List<TicketHistoryEntry>> GetByTicketIdAsync(int ticketId, int limit = 20, CancellationToken ct = default);
        Task<List<TicketHistoryEntry>> GetByCustomerIdAsync(Guid customerId, int limit = 20, CancellationToken ct = default);
    }
}
