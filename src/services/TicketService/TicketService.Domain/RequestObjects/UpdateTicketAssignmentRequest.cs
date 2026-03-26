namespace TicketService.Domain.RequestObjects
{
    public class UpdateTicketAssignmentRequest
    {
        public int TicketId { get; set; }
        public Guid AssignToUserId { get; set; }
    }
}
