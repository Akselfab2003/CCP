using System;
using System.Collections.Generic;
using System.Text;

namespace EmailService.Domain.Models
{
    public class EmailSent
    {
        public int Id { get; set; }
        public Guid OrganizationId { get; set; }
        public required string Subject { get; set; }
        public required string Body { get; set; }
        public required string SenderAddress { get; set; }
        public required string RecipientAddress { get; set; }
        public required DateTime SentAt { get; set; }
        public int? TicketId { get; set; } = null;
    }
}
