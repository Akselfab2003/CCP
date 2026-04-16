using EmailService.Domain.Models;

namespace EmailTemplates.Renderes
{
    public interface IEmailTemplateRenderer
    {
        Task<string> RenderTicketCreatedEmailAsync(
            EmailSent email,
            string organizationName,
            string expectedResponseTime,
            string portalUrl);

        Task<string> RenderTicketReplyEmailAsync(
            EmailReceived email,
            string recipientName,
            string organizationName,
            string agentName,
            string agentRole,
            string ticketStatus,
            string ticketStatusLabel,
            string replyUrl,
            string portalUrl,
            string viewHistoryUrl,
            string reopenUrl);

        Task<string> RenderTicketStatusEmailAsync(
            EmailSent email,
            string organizationName,
            string newStatus,
            string newStatusLabel,
            string oldStatusLabel,
            string updatedByAgent,
            string agentNote,
            string portalUrl,
            string reopenUrl);

        //Task<string> RenderSupportTicketNotificationAsync(
        //    EmailSent email,
        //    string customerEmail,
        //    string organizationName,
        //    string expectedResponseTime,
        //    string managementUrl);

        //Task<string> RenderSupportCustomerReplyNotificationAsync(
        //    EmailReceived email,
        //    string customerName,
        //    string customerEmail,
        //    string organizationName,
        //    string ticketStatus,
        //    string ticketStatusLabel,
        //    string replyUrl,
        //    string managementUrl,
        //    string viewHistoryUrl);
    }
}
