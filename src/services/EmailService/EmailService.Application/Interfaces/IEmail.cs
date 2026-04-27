using CCP.Shared.ValueObjects;
using CustomerService.Sdk.Models;
using EmailService.Domain.Models;
using MessagingService.Sdk.Dtos;

namespace EmailService.Application.Interfaces
{
    public interface IEmail
    {
        Task SendTicketCreatedEmailAsync(
            string to, string subject,
            EmailSent email, int ticketId,
            TicketStatus ticketStatus, string organizationName,
            string expectedResponseTime, string portalUrl,
            TicketOrigin origin);
        Task SendTicketReplyEmailAsync(
            string to, string subject,
            EmailReceived email, int ticketId,
            TicketStatus ticketStatus, CustomerDTO customer,
            string organizationName, string agentName,
            string agentRole, string replyUrl,
            string viewHistoryUrl, TicketOrigin origin);
        Task SendTicketStatusEmailAsync(
            string to, string subject,
            EmailSent email, int ticketId,
            TicketStatus ticketStatus, string organizationName,
            string oldStatusLabel, string portalUrl,
            TicketOrigin origin);
        Task SendSupportCustomerReplyEmailAsync(
            string to, string subject,
            EmailReceived email, int ticketId,
            TicketStatus ticketStatus, CustomerDTO customer,
            string organizationName, string replyUrl,
            string managementUrl, string viewHistoryUrl,
            TicketOrigin origin);

        Task SendReplyToEmailAsync(
            string to,
            string subject,
            List<MessageDto> messages,
            EmailSent emailSent,
            Guid CustomerId,
            Guid OrgId,
            int ticketId,
            TicketStatus ticketStatus,
            string organizationName,
            TicketOrigin origin);
    }
}
