using System;
using System.Collections.Generic;
using System.Text;
using CustomerService.Sdk.Models;
using EmailService.Domain.Models;
using TicketService.Sdk.Dtos;

namespace EmailService.Application.Interfaces
{
    public interface IEmail
    {
        Task SendTicketCreatedEmailAsync(
            string to,string subject,
            EmailSent email, TicketSdkDto ticket,
            string organizationName, string expectedResponseTime,
            string portalUrl);

        Task SendTicketReplyEmailAsync(
            string to,string subject,
            EmailReceived email, TicketSdkDto ticket,
            CustomerDTO customer, string organizationName,
            string agentName, string agentRole,
            string replyUrl, string viewHistoryUrl);

        Task SendTicketStatusEmailAsync(
            string to,string subject,
            EmailSent email, TicketSdkDto ticket,
            string organizationName, string oldStatusLabel,
            string portalUrl);

        Task SendSupportCustomerReplyEmailAsync(
            string to,string subject,
            EmailReceived email, TicketSdkDto ticket,
            CustomerDTO customer, string organizationName,
            string replyUrl, string mangmentUrl,
            string viewHistoryUrl);

        Task SendReplyToEmailAsync(
            string to,string subject,
            EmailReceived emailReceived, EmailSent? emailSent,
            TicketSdkDto ticket, string organizationName);

    }
}
