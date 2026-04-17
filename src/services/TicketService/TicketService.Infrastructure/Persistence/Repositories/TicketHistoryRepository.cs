using Microsoft.EntityFrameworkCore;
using TicketService.Domain.Entities;
using TicketService.Domain.Interfaces;
using TicketService.Infrastructure.Persistence;

namespace TicketService.Infrastructure.Persistence.Repositories
{
    public class TicketHistoryRepository : ITicketHistoryRepository
    {
        private readonly TicketDbContext _context;

        public TicketHistoryRepository(TicketDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(TicketHistoryEntry entry, CancellationToken ct = default)
        {
            _context.TicketHistory.Add(entry);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<List<TicketHistoryEntry>> GetByTicketIdAsync(int ticketId, int limit = 20, CancellationToken ct = default)
        {
            return await _context.TicketHistory
                .Where(h => h.TicketId == ticketId)
                .OrderByDescending(h => h.OccurredAt)
                .Take(limit)
                .ToListAsync(ct);
        }

        public async Task<List<TicketHistoryEntry>> GetByCustomerIdAsync(Guid customerId, int limit = 20, CancellationToken ct = default)
        {
            return await _context.TicketHistory
                .Where(h => _context.Tickets
                    .Where(t => t.CustomerId == customerId)
                    .Select(t => t.Id)
                    .Contains(h.TicketId))
                .OrderByDescending(h => h.OccurredAt)
                .Take(limit)
                .ToListAsync(ct);
        }
    }
}
