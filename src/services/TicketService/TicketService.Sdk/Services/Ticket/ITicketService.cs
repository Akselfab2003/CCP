using TicketService.Sdk.Dtos;

namespace TicketService.Sdk.Services.Ticket
{
    public interface ITicketService
    {
        Task<Result<int>> CreateTicket(CreateTicketRequestDto request, TicketOrigin origin = TicketOrigin.Manual, CancellationToken ct = default);
        Task<Result<TicketSdkDto>> GetTicket(int ticketId, CancellationToken ct = default);
        Task<Result<List<TicketSdkDto>>> GetTickets(Guid? assignedUserId = null, Guid? CustomerId = null, TicketStatus? status = null, CancellationToken ct = default);
        Task<Result> UpdateTicketStatusAsync(int ticketId, TicketStatus newStatus, CancellationToken ct = default);
        Task<Result<List<TicketHistoryEntryDto>>> GetCustomerHistoryAsync(Guid customerId, int limit = 20, CancellationToken ct = default);
        Task<Result> RecordMessageSentAsync(int ticketId, Guid? senderUserId, string messageSnippet, CancellationToken ct = default);
        Task<Result<ManagerStatsSdkDto>> GetManagerStatsAsync(CancellationToken ct = default);
        Task<Result<List<TicketHistoryEntryDto>>> GetOrgHistoryAsync(int limit = 20, CancellationToken ct = default);
        Task<Result<List<TicketHistoryEntryDto>>> GetMyHistoryAsync(int limit = 20, CancellationToken ct = default);
    }
}
