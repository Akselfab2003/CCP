using TicketService.Sdk.Dtos;

namespace TicketService.Sdk.Services.Ticket
{
    public interface ITicketService
    {
        Task<Result<int>> CreateTicket(CreateTicketRequestDto request, CancellationToken ct = default);
        Task<Result<TicketSdkDto>> GetTicket(int ticketId, CancellationToken ct = default);
        Task<Result<List<TicketSdkDto>>> GetTickets(Guid? assignedUserId = null, Guid? CustomerId = null, TicketStatus? status = null, CancellationToken ct = default);
        Task<Result> UpdateTicketStatusAsync(int ticketId, TicketStatus newStatus, CancellationToken ct = default);
    }
}
