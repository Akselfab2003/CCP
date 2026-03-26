namespace TicketService.Application.Services.Assignment
{
    public interface IAssignmentCommands
    {
        Task<Result<Guid>> CreateAssignmentAsync(int ticketId, Guid AssignUserId);
        Task<Result<Guid>> CreateOrUpdateAssignment(int ticketId, Guid assignUserId);
    }
}
