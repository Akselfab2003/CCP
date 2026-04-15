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

            using var client = new MailKit.Net.Smtp.SmtpClient();
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            await client.ConnectAsync(emailHostUrl, 465, true);
            await client.AuthenticateAsync(configuration.GetValue<string>("emailWorkerServiceUsername"), configuration.GetValue<string>("emailWorkerServicePassword"));
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
