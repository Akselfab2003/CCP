using System;
using System.Collections.Generic;
using System.Text;

namespace EmailService.Domain.Models
{
    public class TicketReplyRequest
    {
        public EmailReceived Email { get; set; } = default!;
        public string RecipientName { get; set; } = "Customer";
        public string? OrganizationName { get; set; }
        public string? AgentName { get; set; }
        public string? AgentRole { get; set; }
        public string? TicketStatus { get; set; }
        public string? TicketStatusLabel { get; set; }
        public string? ReplyUrl { get; set; }
        public string? PortalUrl { get; set; }
        public string? ViewHistoryUrl { get; set; }
        public string? ReopenUrl { get; set; }
        public string? SupportTeamEmail { get; set; }
        public string? ManagementUrl { get; set; }
        public bool IsCustomerReply { get; set; } = false;
    }
}
