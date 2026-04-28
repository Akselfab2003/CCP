namespace CCP.Shared.Events
{
    public class TicketCreated
    {
        public int TicketId { get; set; }
        public Guid OrgId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
