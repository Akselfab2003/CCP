using System;
using System.Collections.Generic;
using System.Text;
using MimeKit;
using EmailService.Application.Interfaces;

namespace EmailService.Infrastructure.EmailInfrastructure
{
    public class EmailSendingService :IEmail
    {
        private readonly ISmtpClient _smtpClient;

        public EmailSendingService(ISmtpClient smtpClient)
        {
            _smtpClient = smtpClient;
        }

        public async Task SendEmailNotification(
            Guid userId, string toEmail, string fromEmail,
            string toUser, string text, string subject)
        {
            var message = BuildMessage(fromEmail, userId.ToString(), toEmail, toUser, subject);
            message.Body = new TextPart("plain") { Text = text };
            await _smtpClient.SendAsync(message);
        }

        public async Task SendHtmlEmail(
            string fromAddress, string fromName,
            string toAddress, string toName,
            string subject, string htmlContent)
        {
            var message = BuildMessage(fromAddress, fromName, toAddress, toName, subject);
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
