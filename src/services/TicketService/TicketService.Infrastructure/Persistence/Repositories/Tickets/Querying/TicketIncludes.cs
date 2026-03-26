using Microsoft.EntityFrameworkCore;

namespace TicketService.Infrastructure.Persistence.Repositories.Tickets.Querying
{
    internal static class TicketIncludes
    {
        extension(IQueryable<Domain.Entities.Ticket> query)
        {
            public IQueryable<Domain.Entities.Ticket> IncludeAssignment()
                => query.Include(t => t.Assignment);
        }
    }
}
