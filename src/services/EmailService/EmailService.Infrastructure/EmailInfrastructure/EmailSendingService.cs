using CCP.Shared.ValueObjects;
using CustomerService.Sdk.Models;
using EmailService.Application.Interfaces;
using EmailService.Domain.Models;
using EmailTemplates.Renderes;
using MimeKit;

namespace EmailService.Infrastructure.EmailInfrastructure
{
    public class EmailSendingService : IEmail
    {
        private readonly ISmtpClient _smtpClient;
        private readonly IEmailTemplateRenderer _emailTemplateRenderer;

        public EmailSendingService(ISmtpClient smtpClient, IEmailTemplateRenderer emailTemplateRenderer)
        {
            _smtpClient = smtpClient;
            _emailTemplateRenderer = emailTemplateRenderer;
        }

        public async Task SendTicketCreatedEmailAsync(
            string to, string subject,
            EmailSent email, int ticketId, TicketStatus ticketStatus,
            string organizationName, string expectedResponseTime,
            string portalUrl)
        {
            var htmlContent = await _emailTemplateRenderer
                .RenderTicketCreatedEmailAsync(
                email, ticketId, ticketStatus,
                organizationName, expectedResponseTime,
                portalUrl);

            var message = BuildMessage(
                fromAddress: email.SenderAddress,
                fromName: organizationName,
                toAddress: to,
                toName: email.RecipientAddress,
                subject: subject
            );

            message.Body = new BodyBuilder { HtmlBody = htmlContent }.ToMessageBody();

            await _smtpClient.SendAsync(message);
        }

        public async Task SendTicketReplyEmailAsync(
            string to, string subject,
            EmailReceived email, int ticketId, TicketStatus ticketStatus,

            CustomerDTO customer, string organizationName,
            string agentName, string agentRole,
            string replyUrl, string viewHistoryUrl)
        {
            var htmlContent = await _emailTemplateRenderer
                .RenderTicketReplyEmailAsync(
                email, ticketId, ticketStatus,
                customer, organizationName,
                agentName, agentRole,
                replyUrl, viewHistoryUrl);

            var message = BuildMessage(
                fromAddress: email.SenderAddress,
                fromName: organizationName,
                toAddress: to,
                toName: email.RecipientAddress,
                subject: subject
            );
            message.Body = new BodyBuilder { HtmlBody = htmlContent }.ToMessageBody();
            await _smtpClient.SendAsync(message);
        }

        public async Task SendTicketStatusEmailAsync(
            string to, string subject,
            EmailSent email, int ticketId, TicketStatus ticketStatus,
            string organizationName, string oldStatusLabel,
            string portalUrl)
        {
            var htmlContent = await _emailTemplateRenderer
                .RenderTicketStatusEmailAsync(
                email, ticketId, ticketStatus,
                organizationName, oldStatusLabel,
                portalUrl);

            var message = BuildMessage(
                fromAddress: email.SenderAddress,
                fromName: organizationName,
                toAddress: to,
                toName: email.RecipientAddress,
                subject: subject
            );
            message.Body = new BodyBuilder { HtmlBody = htmlContent }.ToMessageBody();
            await _smtpClient.SendAsync(message);
        }

        public async Task SendSupportCustomerReplyEmailAsync(
            string to, string subject,
            EmailReceived email, int ticketId, TicketStatus ticketStatus,
            CustomerDTO customer, string organizationName,
            string replyUrl, string managementUrl,
            string viewHistoryUrl)
        {
            var htmlContent = await _emailTemplateRenderer
                .RenderSupportCustomerReplyNotificationAsync(
                email, ticketId, ticketStatus,
                customer, organizationName,
                replyUrl, managementUrl,
                viewHistoryUrl);

            var message = BuildMessage(
                fromAddress: email.SenderAddress,
                fromName: organizationName,
                toAddress: to,
                toName: email.RecipientAddress,
                subject: subject
            );
            message.Body = new BodyBuilder { HtmlBody = htmlContent }.ToMessageBody();
            await _smtpClient.SendAsync(message);
        }

        public async Task SendReplyToEmailAsync(
            string to, string subject,
            EmailReceived emailReceived, EmailSent? emailSent,
            int ticketId, TicketStatus ticketStatus, string organizationName)
        {
            var htmlContent = await _emailTemplateRenderer
                .RenderReplyToEmailAsync(
                emailReceived, emailSent,
                ticketId, organizationName);

            var message = BuildMessage(
                fromAddress: emailReceived.SenderAddress,
                fromName: organizationName,
                toAddress: to,
                toName: emailReceived.RecipientAddress,
                subject: subject
            );
            message.Body = new BodyBuilder { HtmlBody = htmlContent }.ToMessageBody();
            await _smtpClient.SendAsync(message);
        }


        private static MimeMessage BuildMessage(
        string fromAddress, string fromName,
        string toAddress, string toName,
        string subject)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromAddress));
            message.To.Add(new MailboxAddress(toName, toAddress));
            message.Subject = subject;
            return message;
        }


    }
}
