using CCP.Shared.ValueObjects;
using CustomerService.Sdk.Models;
using EmailService.Domain.Models;

namespace EmailTemplates.Renderes
{
    public interface IEmailTemplateRenderer
    {
        Task<string> RenderReplyToEmailAsync(EmailReceived emailReceived, EmailSent? emailSent, int ticketId, string organizationName);
        Task<string> RenderSupportCustomerReplyNotificationAsync(EmailReceived email, int ticketId, TicketStatus ticketStatus, CustomerDTO customer, string organizationName, string replyUrl, string mangmentUrl, string viewHistoryUrl);
        Task<string> RenderTicketCreatedEmailAsync(EmailSent email, int ticketId, TicketStatus ticketStatus, string organizationName, string expectedResponseTime, string portalUrl);
        Task<string> RenderTicketReplyEmailAsync(EmailReceived email, int ticketId, TicketStatus ticketStatus, CustomerDTO customer, string organizationName, string agentName, string agentRole, string replyUrl, string viewHistoryUrl);
        Task<string> RenderTicketStatusEmailAsync(EmailSent email, int ticketId, TicketStatus ticketStatus, string organizationName, string oldStatusLabel, string portalUrl);
    }
}
