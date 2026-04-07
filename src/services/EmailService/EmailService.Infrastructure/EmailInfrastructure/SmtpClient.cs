using System;
using System.Collections.Generic;
using System.Text;
using EmailService.Application.Interfaces;
using MimeKit;

namespace EmailService.Infrastructure.EmailInfrastructure
{
    public class SmtpClient : ISmtpClient
    {
        public async Task SendAsync(MimeMessage message)
        {
            using var client = new MailKit.Net.Smtp.SmtpClient();
            await client.ConnectAsync("localhost", 25, false);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
