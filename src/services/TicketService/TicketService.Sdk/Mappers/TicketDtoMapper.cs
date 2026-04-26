using System.Linq.Expressions;
using TicketService.Sdk.Dtos;
using TicketService.Sdk.Models;

namespace TicketService.Sdk.Mappers
{
    internal static class TicketDtoMapper
    {

        private static Guid? AssignmentUserId(TicketDto ticketDto)
        {
            if (ticketDto == null)
                return null;

            if (ticketDto.Assignment == null)
                return null;

            if (ticketDto.Assignment.AssignmentDto == null)
                return null;

            if (ticketDto.Assignment.AssignmentDto.UserId == null)
                return null;

            return ticketDto.Assignment.AssignmentDto.UserId;
        }

        private static Guid? AssignedByUserId(TicketDto ticketDto)
        {
            if (ticketDto == null)
                return null;

            if (ticketDto.Assignment == null)
                return null;

            if (ticketDto.Assignment.AssignmentDto == null)
                return null;

            if (ticketDto.Assignment.AssignmentDto.AssignedByUserId == null)
                return null;

            return ticketDto.Assignment.AssignmentDto.AssignedByUserId;
        }


        public static readonly Expression<Func<TicketDto, TicketSdkDto>> TicketProjection = t => new TicketSdkDto()
        {
            Id = t.Id ?? 0,
            Title = t.Title ?? string.Empty,
            AssignedUserId = AssignmentUserId(t),
            Status = t.Status ?? 0,
            Description = t.Description ?? string.Empty,
            OrganizationId = t.OrganizationId ?? Guid.Empty,
            AssignedByUserId = AssignedByUserId(t),
            CreatedAt = t.CreatedAt,
            CustomerId = t.CustomerId,
        };


        public static TicketSdkDto ToDto(this TicketDto ticket)
               => TicketProjection.Compile().Invoke(ticket);

        public static List<TicketSdkDto> ToDto(this IEnumerable<TicketDto> tickets)
            => tickets.Select(t => t.ToDto()).ToList();

    }
}
