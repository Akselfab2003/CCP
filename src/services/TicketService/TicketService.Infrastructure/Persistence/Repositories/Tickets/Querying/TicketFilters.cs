using ChatApp.Shared.ValueObjects;

namespace TicketService.Infrastructure.Persistence.Repositories.Tickets.Querying
{
    internal static class TicketFilters
    {
        extension(IQueryable<Domain.Entities.Ticket> query)
        {
            public IQueryable<Domain.Entities.Ticket> ById(int id)
                => query.Where(t => t.Id == id);

            public IQueryable<Domain.Entities.Ticket> ByStatus(TicketStatus status)
                => query.Where(t => t.Status == status);

            public IQueryable<Domain.Entities.Ticket> HasAssignment()
                => query.Where(t => t.Assignment != null);

            public IQueryable<Domain.Entities.Ticket> ByAssignedUserId(Guid assignedUserId)
                => query.HasAssignment().Where(t => t.Assignment!.UserId == assignedUserId);

            public IQueryable<Domain.Entities.Ticket> ByCustomerId(Guid customerId)
                => query.Where(t => t.CustomerId == customerId);

            public IQueryable<Domain.Entities.Ticket> Query(int? id = null, Guid? assignedUserId = null, Guid? CustomerId = null, TicketStatus? status = null)
            {
                var filteredQuery = query;

                if (id.HasValue)
                    filteredQuery = filteredQuery.ById(id.Value);

                if (assignedUserId.HasValue)
                    filteredQuery = filteredQuery.ByAssignedUserId(assignedUserId.Value);

                if (CustomerId.HasValue)
                    filteredQuery = filteredQuery.ByCustomerId(CustomerId.Value);

                if (status.HasValue)
                    filteredQuery = filteredQuery.ByStatus(status.Value);

                return filteredQuery;
            }
        }
    }
}
