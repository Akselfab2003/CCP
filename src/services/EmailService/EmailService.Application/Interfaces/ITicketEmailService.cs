using System;
using System.Collections.Generic;
using System.Text;
using EmailService.Domain.Models;

namespace EmailService.Application.Interfaces
{
    public interface ITicketEmailService
    {
        Task SendTicketCreatedNotificationAsync(
            string recipientEmail,
            string ticketTitle,
            EmailSent emailModel,
            string organizationName,
            string expectedResponseTime,
            string portalUrl);

        Task SendTicketReplyNotificationAsync(
            string recipientEmail,
            string ticketTitle,
            EmailReceived emailModel,
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

        Task SendTicketStatusChangeNotificationAsync(
            string recipientEmail,
            string ticketTitle,
            EmailSent emailModel,
            string organizationName,
            string newStatus,
            string newStatusLabel,
            string oldStatusLabel,
            string updatedByAgent,
            string agentNote,
            string portalUrl,
            string reopenUrl);
        Task SendSupportCustomerReplyNotificationAsync(
            string recipientEmail,
            EmailReceived emailModel,
            string customerName,
            string customerEmail,
            string organizationName,
            string ticketStatus,
            string ticketStatusLabel,
            string replyUrl,
            string managementUrl,
            string viewHistoryUrl);

        Task SendReplyToEmailAsync(
            string recipientEmail,
            EmailReceived emailReceived,
            EmailSent emailSent,
            int ticketId,
            string organizationName);

    }
}
