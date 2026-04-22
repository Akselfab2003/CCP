using CustomerService.Domain.Entities;
using CustomerService.Sdk.Models;
using EmailService.Domain.Models;
using TicketService.Domain.Entities;
using TicketService.Domain.ResponseObjects;
using TicketService.Sdk.Dtos;

namespace EmailTemplates.Renderes
{
    public interface IEmailTemplateRenderer
    {
        Task<string> RenderTicketCreatedEmailAsync(
            EmailSent email, TicketSdkDto ticket,
            string organizationName, string expectedResponseTime,
            string portalUrl);

        Task<string> RenderTicketReplyEmailAsync(
            EmailReceived email, TicketSdkDto ticket,
            CustomerDTO customer, string organizationName,
            string agentName, string agentRole,
            string replyUrl, string viewHistoryUrl);

        Task<string> RenderTicketStatusEmailAsync(
            EmailSent email, TicketSdkDto ticket,
            string organizationName, string OldStatusLabel,
            string portalUrl);

        Task<string> RenderSupportCustomerReplyNotificationAsync(
            EmailReceived email, TicketSdkDto ticket,
            CustomerDTO customer, string organizationName,
            string replyUrl, string mangmentUrl,
            string viewHistoryUrl);

        Task<string> RenderReplyToEmailAsync(
            EmailReceived emailReceived,EmailSent? emailSent,
            TicketSdkDto ticket,string organizationName
            );
    }
}
