using EmailService.Domain.Models;

namespace EmailService.Sdk.Services
{
    public interface IEmailService
    {

        Task SendTicketCreatedEmailAsync(
            int ticketId,
            string subject,
            string body,
            string recipientEmail,
            string organizationName = "Support",
            string expectedResponseTime = "24 hours",
            string portalUrl = "#");

        Task SendTicketReplyEmailAsync(
            int ticketId,
            string subject,
            string body,
            string recipientEmail,
            string recipientName,
            string agentName,
            string agentRole = "Support Agent",
            string organizationName = "Support",
            string ticketStatus = "open",
            string ticketStatusLabel = "Open",
            string replyUrl = "#",
            string portalUrl = "#",
            string viewHistoryUrl = "#",
            string reopenUrl = "#");

        Task SendTicketStatusEmailAsync(
            int ticketId,
            string subject,
            string recipientEmail,
            string newStatus,
            string newStatusLabel,
            string oldStatusLabel = "Open",
            string organizationName = "Support",
            string updatedByAgent = "",
            string agentNote = "",
            string portalUrl = "#",
            string reopenUrl = "#");
    }
}
