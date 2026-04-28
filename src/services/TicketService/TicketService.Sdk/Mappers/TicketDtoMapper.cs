using System.Linq.Expressions;
using CCP.Shared.ValueObjects;
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
            AssignedByUserId = t.Assignment == null ? null : t.Assignment.AssignedByUserId,
            Status = t.Status ?? 0,
            OrganizationId = t.OrganizationId ?? Guid.Empty,
            CreatedAt = t.CreatedAt,
            CustomerId = t.CustomerId,
            Origin = (TicketOrigin)(t.Origin ?? 0),
        };

        public static TicketSdkDto ToDto(this TicketDto ticket)
               => TicketProjection.Compile().Invoke(ticket);

        public static List<TicketSdkDto> ToDto(this IEnumerable<TicketDto> tickets)
            => tickets.Select(t => t.ToDto()).ToList();
    }
}
