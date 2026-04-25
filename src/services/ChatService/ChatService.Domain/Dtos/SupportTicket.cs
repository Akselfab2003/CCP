namespace ChatService.Domain.Dtos
{
    public class SupportTicket
    {
        public int TicketId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<TicketMessage> Messages { get; set; } = [];
    }
}
