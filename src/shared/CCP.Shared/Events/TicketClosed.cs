namespace CCP.Shared.Events
{
    public class TicketClosed
    {
        public int TicketId { get; set; }
        public Guid OrgId { get; set; }
        public DateTime ClosedAt { get; set; } = DateTime.Now;
    }
}
