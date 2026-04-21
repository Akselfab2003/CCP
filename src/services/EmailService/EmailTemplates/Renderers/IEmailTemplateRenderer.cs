using CustomerService.Domain.Entities;
using EmailService.Domain.Models;
using TicketService.Domain.Entities;

namespace EmailTemplates.Renderes
{
    public interface IEmailTemplateRenderer
    {
        Task<string> RenderTicketCreatedEmailAsync(
            EmailSent email, Ticket ticket,
            string organizationName, string expectedResponseTime,
            string portalUrl);

        Task<string> RenderTicketReplyEmailAsync(
            EmailReceived email, Ticket ticket,
            Customer customer, string organizationName,
            string agentName, string agentRole,
            string replyUrl, string viewHistoryUrl);

        Task<string> RenderTicketStatusEmailAsync(
            EmailSent email, Ticket ticket,
            string organizationName, string OldStatusLabel,
            string portalUrl);

        Task<string> RenderSupportCustomerReplyNotificationAsync(
            EmailReceived email,Ticket ticket,
            Customer customer, string organizationName,
            string replyUrl, string mangmentUrl,
            string viewHistoryUrl);

        Task<string> RenderReplyToEmailAsync(
            EmailReceived emailReceived,EmailSent emailSent,
            Ticket ticket,string organizationName
            );
    }
}
