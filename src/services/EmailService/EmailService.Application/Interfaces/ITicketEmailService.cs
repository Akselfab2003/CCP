using CCP.Shared.ValueObjects;
using CustomerService.Sdk.Models;
using EmailService.Domain.Models;
using MessagingService.Sdk.Dtos;


namespace EmailService.Application.Interfaces
{
    public interface ITicketEmailService
    {
        Task SendTicketCreatedNotificationAsync(
            string recipientEmail, string ticketTitle,
            EmailSent emailModel, int ticketId,
            TicketStatus ticketStatus, string organizationName,
            string expectedResponseTime, string portalUrl,
            TicketOrigin origin);
        Task SendTicketReplyNotificationAsync(
            string recipientEmail, string ticketTitle,
            EmailReceived emailModel, int ticketId,
            TicketStatus ticketStatus, CustomerDTO customer,
            string organizationName, string agentName,
            string agentRole, string replyUrl,
            string viewHistoryUrl, TicketOrigin origin);
        Task SendTicketStatusChangeNotificationAsync(
            string recipientEmail, string ticketTitle,
            EmailSent emailModel, int ticketId,
            TicketStatus ticketStatus, string organizationName,
            string oldStatusLabel, string portalUrl,
            TicketOrigin origin);
        Task SendSupportCustomerReplyNotificationAsync(
            string recipientEmail, EmailReceived emailModel,
            int ticketId, TicketStatus ticketStatus,
            CustomerDTO customer, string organizationName,
            string replyUrl, string managementUrl,
            string viewHistoryUrl, TicketOrigin origin);
        Task SendReplyToEmailAsync(
            string recipientEmail, EmailSent email,
            List<MessageDto> messages, int ticketId,
            string organizationName, TicketOrigin origin,
            TicketStatus ticketStatus);
    }
}
