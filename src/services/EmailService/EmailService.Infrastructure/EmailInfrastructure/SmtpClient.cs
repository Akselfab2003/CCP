using EmailService.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace EmailService.Infrastructure.EmailInfrastructure
{
    public class SmtpClient : ISmtpClient
    {
        private readonly IConfiguration configuration;

        public SmtpClient(IConfiguration configuration) => this.configuration = configuration;

        public async Task SendAsync(MimeMessage message)
        {
            var emailHostUrl = configuration.GetValue<string>("emailHostUrl") ?? throw new InvalidOperationException("emailHostUrl configuration value is required.");

            var username = configuration.GetValue<string>("emailWorkerServiceUsername") ?? throw new InvalidOperationException("emailWorkerServiceUsername configuration value is required.");
            var password = configuration.GetValue<string>("emailWorkerServicePassword") ?? throw new InvalidOperationException("emailWorkerServicePassword configuration value is required.");

            using var client = new MailKit.Net.Smtp.SmtpClient();
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            await client.ConnectAsync(emailHostUrl, 465, true);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
