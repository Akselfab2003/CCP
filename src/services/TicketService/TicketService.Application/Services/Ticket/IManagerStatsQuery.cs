namespace TicketService.Application.Services.Ticket
{
    public interface IManagerStatsQuery
    {
        Task<ManagerStatsDto> GetManagerStatsAsync(Guid assignedUserId, CancellationToken ct = default);
    }
}
