using CCP.Shared.ValueObjects;
using CustomerService.Sdk.Models;
using EmailService.Domain.Models;


namespace EmailService.Application.Interfaces
{
    public interface ITicketEmailService
    {
        Task SendTicketCreatedNotificationAsync(string recipientEmail, string ticketTitle, EmailSent emailModel, int ticketId, TicketStatus ticketStatus, string organizationName, string expectedResponseTime, string portalUrl);
        Task SendTicketReplyNotificationAsync(string recipientEmail, string ticketTitle, EmailReceived emailModel, int ticketId, TicketStatus ticketStatus, CustomerDTO customer, string organizationName, string agentName, string agentRole, string replyUrl, string viewHistoryUrl);
        Task SendTicketStatusChangeNotificationAsync(string recipientEmail, string ticketTitle, EmailSent emailModel, int ticketId, TicketStatus ticketStatus, string organizationName, string oldStatusLabel, string portalUrl);
        Task SendSupportCustomerReplyNotificationAsync(string recipientEmail, EmailReceived emailModel, int ticketId, TicketStatus ticketStatus, CustomerDTO customer, string organizationName, string replyUrl, string managementUrl, string viewHistoryUrl);
        Task SendReplyToEmailAsync(string recipientEmail, EmailReceived emailReceived, EmailSent emailSent, int ticketId, TicketStatus ticketStatus, string organizationName);
    }
}
