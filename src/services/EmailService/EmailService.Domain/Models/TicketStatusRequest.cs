using System;
using System.Collections.Generic;
using System.Text;

namespace EmailService.Domain.Models
{
    public class TicketStatusRequest
    {
        public EmailSent Email { get; set; } = default!;
        public string RecipientName { get; set; } = "Customer";
        public string? OrganizationName { get; set; }
        public string? NewStatus { get; set; }
        public string? NewStatusLabel { get; set; }
        public string? OldStatusLabel { get; set; }
        public string? UpdatedByAgent { get; set; }
        public string? AgentNote { get; set; }
        public string? PortalUrl { get; set; }
        public string? ReopenUrl { get; set; }
    }
}
