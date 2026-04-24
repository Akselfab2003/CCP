using Microsoft.EntityFrameworkCore;
using TicketService.Application.Services.Ticket;
using TicketService.Infrastructure.Persistence;

namespace TicketService.Infrastructure.Persistence.Repositories.Tickets
{
    internal class ManagerStatsQuery : IManagerStatsQuery
    {
        private readonly TicketDbContext _context;

        public ManagerStatsQuery(TicketDbContext context)
        {
            _context = context;
        }

        public async Task<ManagerStatsDto> GetManagerStatsAsync(Guid assignedUserId, CancellationToken ct = default)
        {
            var openTickets = await _context.Tickets
                .CountAsync(t => t.Status != TicketStatus.Closed && t.Status != TicketStatus.Blocked, ct);

            var since = DateTimeOffset.UtcNow.AddHours(-24);
            var closedToday = await _context.TicketHistory
                .Where(h => h.EventType == "StatusChanged"
                         && h.NewValue == "Closed"
                         && h.OccurredAt >= since
                         && _context.Tickets.Any(t => t.Id == h.TicketId))
                .Select(h => h.TicketId)
                .Distinct()
                .CountAsync(ct);

            var avgResponseTime = await ComputeAvgResponseTimeAsync(ct);

            var awaitingUser = await _context.Tickets
                .CountAsync(t => t.Status == TicketStatus.WaitingForSupport
                              && _context.Assignments.Any(a => a.TicketId == t.Id && a.UserId == assignedUserId), ct);

            var sevenDaysAgo = DateTimeOffset.UtcNow.AddDays(-7);
            var teamPerformance = await _context.TicketHistory
                .Where(h => h.EventType == "StatusChanged"
                         && h.NewValue == "Closed"
                         && h.OccurredAt >= sevenDaysAgo
                         && _context.Tickets.Any(t => t.Id == h.TicketId))
                .Join(_context.Assignments,
                      h => h.TicketId,
                      a => a.TicketId,
                      (h, a) => new { a.UserId, h.TicketId })
                .GroupBy(x => x.UserId)
                .Select(g => new SupporterPerformanceDto
                {
                    UserId = g.Key,
                    ResolvedCount = g.Select(x => x.TicketId).Distinct().Count()
                })
                .OrderByDescending(x => x.ResolvedCount)
                .ToListAsync(ct);

            return new ManagerStatsDto
            {
                OpenTickets = openTickets,
                ClosedToday = closedToday,
                AvgResponseTime = avgResponseTime,
                AwaitingUser = awaitingUser,
                TeamPerformance = teamPerformance
            };
        }

        private async Task<string> ComputeAvgResponseTimeAsync(CancellationToken ct)
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            var firstResponses = await _context.Tickets
                .Where(t => t.CreatedAt >= thirtyDaysAgo)
                .Join(_context.TicketHistory,
                      t => t.Id,
                      h => h.TicketId,
                      (t, h) => new { t.Id, t.CreatedAt, t.CustomerId, h.EventType, h.ActorUserId, h.OccurredAt })
                .Where(x => x.EventType == "MessageSent" && x.ActorUserId != x.CustomerId)
                .GroupBy(x => new { x.Id, x.CreatedAt })
                .Select(g => new
                {
                    g.Key.Id,
                    g.Key.CreatedAt,
                    FirstResponseAt = g.Min(x => x.OccurredAt)
                })
                .ToListAsync(ct);

            if (firstResponses.Count == 0)
                return "—";

            var avgMinutes = firstResponses
                .Select(r => (r.FirstResponseAt.UtcDateTime - r.CreatedAt).TotalMinutes)
                .Where(m => m > 0)
                .DefaultIfEmpty(0)
                .Average();

            if (avgMinutes < 60)
                return $"{(int)avgMinutes}m";
            if (avgMinutes < 1440)
                return $"{(int)(avgMinutes / 60)}h {(int)(avgMinutes % 60)}m";
            return $"{(int)(avgMinutes / 1440)}d {(int)((avgMinutes % 1440) / 60)}h";
        }
    }
}
