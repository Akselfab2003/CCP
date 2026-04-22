namespace EmailService.Domain.Models
{
    public class EmailTicketEntities
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public int TicketId { get; set; }
        public Guid CustomerId { get; set; }
        public required string MailId { get; set; }
        public required string SenderEmail { get; set; }
        public List<string> MailReferenceIds { get; set; } = [];
    }
}
