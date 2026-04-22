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
            string to,
            string subject,
            EmailSent email,
            string organizationName,
            string expectedResponseTime,
            string portalUrl)
        {
            throw new NotImplementedException();
            //var htmlContent = await _emailTemplateRenderer.RenderTicketCreatedEmailAsync(
            //    email, organizationName, expectedResponseTime, portalUrl);

            //var message = BuildMessage(
            //    fromAddress: email.SenderAddress,
            //    fromName: organizationName,
            //    toAddress: to,
            //    toName: email.RecipientAddress,
            //    subject: subject
            //);

            //message.Body = new BodyBuilder { HtmlBody = htmlContent }.ToMessageBody();

            //await _smtpClient.SendAsync(message);
        }

        public async Task SendTicketReplyEmailAsync(
            string to,
            string subject,
            EmailReceived email,
            string organizationName,
            string recipientName,
            string agentName,
            string agentRole,
            string ticketStatus,
            string ticketStatusLabel,
            string replyUrl,
            string portalUrl,
            string viewHistoryUrl,
            string reopenUrl)
        {
            throw new NotImplementedException();
            //var htmlContent = await _emailTemplateRenderer.RenderTicketReplyEmailAsync(
            //    email, organizationName, recipientName, agentName, agentRole, ticketStatus, ticketStatusLabel, replyUrl, portalUrl, viewHistoryUrl, reopenUrl);
            //var message = BuildMessage(
            //    fromAddress: email.SenderAddress,
            //    fromName: organizationName,
            //    toAddress: to,
            //    toName: email.RecipientAddress,
            //    subject: subject
            //);
            //message.Body = new BodyBuilder { HtmlBody = htmlContent }.ToMessageBody();
            //await _smtpClient.SendAsync(message);
        }

        public async Task SendTicketStatusEmailAsync(
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
            string reopenUrl)
        {
            throw new NotImplementedException();
            //var htmlContent = await _emailTemplateRenderer.RenderTicketStatusEmailAsync(
            //    email, organizationName, newStatus, newStatusLabel, oldStatusLabel, updatedByAgent, agentNote, portalUrl, reopenUrl);
            //var message = BuildMessage(
            //    fromAddress: email.SenderAddress,
            //    fromName: organizationName,
            //    toAddress: to,
            //    toName: email.RecipientAddress,
            //    subject: subject
            //);
            //message.Body = new BodyBuilder { HtmlBody = htmlContent }.ToMessageBody();
            //await _smtpClient.SendAsync(message);
        }

        public async Task SendSupportCustomerReplyEmailAsync(
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
            string viewHistoryUrl)
        {
            throw new NotImplementedException();
            //var htmlContent = await _emailTemplateRenderer.RenderSupportCustomerReplyNotificationAsync(email: email,
            //                                                                                           customerName,
            //                                                                                           customerEmail,
            //                                                                                           organizationName,
            //                                                                                           ticketStatus,
            //                                                                                           ticketStatusLabel,
            //                                                                                           replyUrl,
            //                                                                                           managementUrl,
            //                                                                                           viewHistoryUrl);
            //var message = BuildMessage(
            //    fromAddress: email.SenderAddress,
            //    fromName: organizationName,
            //    toAddress: to,
            //    toName: email.RecipientAddress,
            //    subject: subject
            //);
            //message.Body = new BodyBuilder { HtmlBody = htmlContent }.ToMessageBody();
            //await _smtpClient.SendAsync(message);
        }

        public async Task SendReplyToEmailAsync(
            string to,
            string subject,
            EmailReceived emailReceived,
            EmailSent? emailSent,
            int ticketId,
            string organizationName)
        {
            throw new NotImplementedException();
            //var htmlContent = await _emailTemplateRenderer.RenderReplyToEmailAsync(
            //    emailReceived, emailSent, ticketId, organizationName);
            //var message = BuildMessage(
            //    fromAddress: emailSent.SenderAddress,
            //    fromName: organizationName,
            //    toAddress: to,
            //    toName: emailReceived.RecipientAddress,
            //    subject: subject
            //);
            //message.Body = new BodyBuilder { HtmlBody = htmlContent }.ToMessageBody();
            //await _smtpClient.SendAsync(message);
        }


        private static MimeMessage BuildMessage(
        string fromAddress,
        string fromName,
        string toAddress,
        string toName,
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
