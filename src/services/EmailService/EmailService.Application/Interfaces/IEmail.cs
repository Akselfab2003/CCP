using CCP.Shared.ValueObjects;
using CustomerService.Sdk.Models;
using EmailService.Domain.Models;

namespace EmailService.Application.Interfaces
{
    public interface IEmail
    {
        Task SendTicketCreatedEmailAsync(string to, string subject, EmailSent email, int ticketId, TicketStatus ticketStatus, string organizationName, string expectedResponseTime, string portalUrl);
        Task SendTicketReplyEmailAsync(string to, string subject, EmailReceived email, int ticketId, TicketStatus ticketStatus, CustomerDTO customer, string organizationName, string agentName, string agentRole, string replyUrl, string viewHistoryUrl);
        Task SendTicketStatusEmailAsync(string to, string subject, EmailSent email, int ticketId, TicketStatus ticketStatus, string organizationName, string oldStatusLabel, string portalUrl);
        Task SendSupportCustomerReplyEmailAsync(string to, string subject, EmailReceived email, int ticketId, TicketStatus ticketStatus, CustomerDTO customer, string organizationName, string replyUrl, string managementUrl, string viewHistoryUrl);
        Task SendReplyToEmailAsync(string to, string subject, EmailReceived emailReceived, EmailSent? emailSent, int ticketId, TicketStatus ticketStatus, string organizationName);
    }
}
