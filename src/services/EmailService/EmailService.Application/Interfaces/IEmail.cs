using System;
using System.Collections.Generic;
using System.Text;

namespace EmailService.Application.Interfaces
{
    public interface IEmail
    {
        Task SendEmailNotification(Guid userId, string toEmail, string fromEmail, string toUser, string text, string subject);
        Task SendHtmlEmail(string fromAddress, string fromName, string toAddress, string toName, string subject, string htmlContent);
    }
}
