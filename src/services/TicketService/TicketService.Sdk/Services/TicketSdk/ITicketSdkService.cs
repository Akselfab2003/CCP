using TicketService.Sdk.Dtos;

namespace TicketService.Sdk.Services.TicketSdk
{
    public interface ITicketSdkService
    {
        Task<Result<TicketSdkDto>> GetTicketAsync(int ticketId, CancellationToken ct = default);
        Task<Result<List<TicketSdkDto>>> GetTicketsAsync(Guid? assignedUserId = null, Guid? customerId = null, string? status = null, CancellationToken ct = default);
        Task<Result<int>> CreateTicketAsync(string title, Guid? customerId, Guid? assignedUserId, CancellationToken ct = default);
        Task<Result> AssignTicketAsync(int ticketId, Guid userId, CancellationToken ct = default);
        Task<Result> UpdateTicketStatusAsync(int ticketId, TicketStatus newStatus, CancellationToken ct = default);
    }
}
