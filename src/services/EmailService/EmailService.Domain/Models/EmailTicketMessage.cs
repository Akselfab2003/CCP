namespace EmailService.Domain.Models
{
    public class EmailTicketMessage
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public int TicketId { get; set; }
        public Guid CustomerId { get; set; }

        public required string MessageId { get; set; }
        public required string InReplyTo { get; set; }
        public List<string> References { get; set; } = [];

        public required string SenderEmail { get; set; }
        public required string SenderName { get; set; }


        public required string Subject { get; set; }
        public string Body { get; set; } = string.Empty;

        public DateTime SentAt { get; set; }
        public EmailDirection Direction { get; set; }
    }

    public enum EmailDirection
    {
        Inbound,
        Outbound
    }
}
