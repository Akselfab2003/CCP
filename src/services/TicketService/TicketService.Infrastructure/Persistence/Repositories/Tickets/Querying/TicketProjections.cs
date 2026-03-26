using System.Linq.Expressions;
using TicketService.Domain.Entities;
using TicketService.Domain.ResponseObjects;

namespace TicketService.Infrastructure.Persistence.Repositories.Tickets.Querying
{
    internal static class TicketProjections
    {
        public static readonly Expression<Func<Ticket, TicketDto>> TicketProjection = t => new TicketDto()
        {
            Id = t.Id,
            Title = t.Title,
            Status = t.Status,
            OrganizationId = t.OrganizationId,
            CreatedAt = t.CreatedAt,
            CustomerId = t.CustomerId,
            InternalNotes = t.InternalNotes,
            Assignment = t.Assignment == null ? null : ToDto(t.Assignment)
        };

        public static readonly Expression<Func<Assignment, AssignmentDto>> AssignmentProjection = a => new AssignmentDto()
        {
            Id = a.Id,
            UserId = a.UserId,
            AssignedByUserId = a.AssignByUserId,
            UpdatedAt = a.UpdatedAt
        };

        public static AssignmentDto ToDto(this Assignment assignment)
            => AssignmentProjection.Compile().Invoke(assignment);

        public static IQueryable<TicketDto> ToDto(this IQueryable<Ticket> query)
            => query.Select(TicketProjection);
        public static IQueryable<AssignmentDto> ToDto(this IQueryable<Assignment> query)
           => query.Select(AssignmentProjection);
    }
}
