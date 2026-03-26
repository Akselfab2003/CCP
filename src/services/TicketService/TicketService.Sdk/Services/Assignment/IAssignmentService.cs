namespace TicketService.Sdk.Services.Assignment
{
    internal interface IAssignmentService
    {
        Task AssignTicketToUserAsync(int ticketId, Guid userId, CancellationToken cancellationToken = default);
    }
}