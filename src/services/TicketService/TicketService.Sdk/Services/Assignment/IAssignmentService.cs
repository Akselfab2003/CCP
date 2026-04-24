namespace TicketService.Sdk.Services.Assignment
{
    public interface IAssignmentService
    {
        Task<Result> AssignTicketToUserAsync(int ticketId, Guid userId, CancellationToken cancellationToken = default);
    }
}
