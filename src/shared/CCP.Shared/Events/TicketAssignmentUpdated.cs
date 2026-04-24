namespace CCP.Shared.Events
{
    public class TicketAssignmentUpdated
    {
        public int ticketId { get; set; }
        public Guid assignedUserId { get; set; }
    }
}
