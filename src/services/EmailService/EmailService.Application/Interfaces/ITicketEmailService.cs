using System;
using System.Collections.Generic;
using System.Text;
using CustomerService.Sdk.Models;
using EmailService.Domain.Models;
using TicketService.Sdk.Dtos;


namespace EmailService.Application.Interfaces
{
    public interface ITicketEmailService
    {
        Task SendTicketCreatedNotificationAsync(
            string recipientEmail, string ticketTitle,
            EmailSent emailModel, TicketSdkDto ticket,
            string organizationName, string expectedResponseTime,
            string portalUrl);

        Task SendTicketReplyNotificationAsync(
            string recipientEmail, string ticketTitle,
            EmailReceived emailModel, TicketSdkDto ticket,
            CustomerDTO customer, string organizationName,
            string agentName, string agentRole,
            string replyUrl, string viewHistoryUrl);

        Task SendTicketStatusChangeNotificationAsync(
            string recipientEmail, string ticketTitle,
            EmailSent emailModel, TicketSdkDto ticket,
            string organizationName, string oldStatusLabel,
            string portalUrl);

        Task SendSupportCustomerReplyNotificationAsync(
            string recipientEmail,EmailReceived emailModel,
            TicketSdkDto ticket, CustomerDTO customer,
            string organizationName, string replyUrl,
            string managementUrl, string viewHistoryUrl);

        Task SendReplyToEmailAsync(
            string recipientEmail, EmailReceived emailReceived,
            EmailSent emailSent, TicketSdkDto ticket,
            string organizationName);

    }
}
