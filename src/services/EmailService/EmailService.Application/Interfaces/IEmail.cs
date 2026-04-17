using System;
using System.Collections.Generic;
using System.Text;
using EmailService.Domain.Models;

namespace EmailService.Application.Interfaces
{
    public interface IEmail
    {
        Task SendTicketCreatedEmailAsync(
            string to,
            string subject,
            EmailSent email,
            string organizationName,
            string expectedResponseTime,
            string portalUrl);

        Task SendTicketReplyEmailAsync(
            string to,
            string subject,
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

        Task SendTicketStatusEmailAsync(
            string to,
            string subject,
            EmailSent email,
            string organizationName,
            string newStatus,
            string newStatusLabel,
            string oldStatusLabel,
            string updatedByAgent,
            string agentNote,
            string portalUrl,
            string reopenUrl);

        Task SendSupportCustomerReplyEmailAsync(
            string to,
            string subject,
            EmailReceived email,
            string customerName,
            string customerEmail,
            string organizationName,
            string ticketStatus,
            string ticketStatusLabel,
            string replyUrl,
            string managementUrl,
            string viewHistoryUrl);

        Task SendSupportNewTicketEmailAsync(
            string to,
            string subject,
            EmailSent email,
            string customerEmail,
            string organizationName,
            string expectedResponseTime,
            string managementUrl);

    }
}
