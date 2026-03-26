using System;
using System.Collections.Generic;
using System.Text;

namespace EmailService.Domain.Models
{
    public class TicketCreatedRequest
    {
        public EmailSent Email { get; set; } = default!;
        public string RecipientName { get; set; } = "Customer";
        public string? OrganizationName { get; set; }
        public string? ExpectedResponseTime { get; set; }
        public string? PortalUrl { get; set; }
        public string? SupportTeamEmail { get; set; }
        public string? ManagementUrl { get; set; }
    }
}
