using System.Linq.Expressions;
using TicketService.Sdk.Dtos;
using TicketService.Sdk.Models;

namespace TicketService.Sdk.Mappers
{
    internal static class TicketDtoMapper
    {
        public static readonly Expression<Func<TicketDto, TicketSdkDto>> TicketProjection = t => new TicketSdkDto()
        {
            Id = t.Id ?? 0,
            Title = t.Title ?? string.Empty,
            AssignedUserId = t.Assignment == null ? null : t.Assignment.UserId,
            Status = t.Status ?? 0,
            OrganizationId = t.OrganizationId ?? Guid.Empty,
            AssignedByUserId = t.Assignment == null ? null : t.Assignment.AssignedByUserId,
            CreatedAt = t.CreatedAt,
            CustomerId = t.CustomerId,
        };


        public static TicketSdkDto ToDto(this TicketDto ticket)
               => TicketProjection.Compile().Invoke(ticket);

        public static List<TicketSdkDto> ToDto(this IEnumerable<TicketDto> tickets)
            => tickets.Select(t => t.ToDto()).ToList();

    }
}
