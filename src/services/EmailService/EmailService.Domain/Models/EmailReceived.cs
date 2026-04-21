using System;
using System.Collections.Generic;
using System.Text;

namespace EmailService.Domain.Models
{
    public class EmailReceived
    {
        public int Id { get; set; }
        public string MailId { get; set; } = string.Empty;
        public Guid OrganizationId { get; set; }
        public required string Subject { get; set; }
        public required string Body { get; set; }
        public required string SenderAddress { get; set; }
        public required string RecipientAddress { get; set; }
        public required DateTime ReceivedAt { get; set; }
    }
}
