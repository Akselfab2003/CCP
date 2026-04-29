namespace CCP.Shared.Events
{
    public class TicketMessageReceived
    {
        public int TicketId { get; set; }
        public Guid OrgId { get; set; }

        public DateTime ReceivedAt { get; set; } = DateTime.Now;
    }
}
