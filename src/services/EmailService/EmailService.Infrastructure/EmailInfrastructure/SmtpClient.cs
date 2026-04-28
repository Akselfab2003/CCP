using CCP.Shared.AuthContext;
using CCP.Shared.ResultAbstraction;
using EmailService.Application.Interfaces;
using EmailService.Domain.Interfaces;
using EmailService.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace EmailService.Infrastructure.EmailInfrastructure
{
    public class SmtpClient : ISmtpClient
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpClient> _logger;
        private readonly ITenantEmailConfigurationRepo _tenantEmailConfigurationRepo;
        private readonly ICurrentUser _currentUser;


        public SmtpClient(IConfiguration configuration, ILogger<SmtpClient> logger, ITenantEmailConfigurationRepo tenantEmailConfigurationRepo, ICurrentUser currentUser)
        {
            _configuration = configuration;
            _logger = logger;
            _tenantEmailConfigurationRepo = tenantEmailConfigurationRepo;
            _currentUser = currentUser;
        }

        public async Task SendAsync(MimeMessage message)
        {
            string username;
            string password;
            var emailHostUrl = _configuration.GetValue<string>("emailHostUrl") ?? throw new InvalidOperationException("emailHostUrl configuration value is required.");

            var tenantEmailConfig = await GetTenantEmailConfiguration();
            if (tenantEmailConfig.IsFailure)
            {
                _logger.LogWarning("Failed to retrieve tenant email configuration for tenant with id {TenantId}. Error: {Error}. Falling back to default SMTP credentials.", _currentUser.OrganizationId, tenantEmailConfig.Error);
                username = _configuration.GetValue<string>("emailWorkerServiceUsername") ?? throw new InvalidOperationException("emailWorkerServiceUsername configuration value is required.");
                password = _configuration.GetValue<string>("emailWorkerServicePassword") ?? throw new InvalidOperationException("emailWorkerServicePassword configuration value is required.");
            }
            else
            {
                username = tenantEmailConfig.Value.InternalEmail;
                password = tenantEmailConfig.Value.InternalEmailPassword;
            }


            using var client = new MailKit.Net.Smtp.SmtpClient();
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;
            await client.ConnectAsync(emailHostUrl, 465, true);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        private async Task<Result<TenantEmailConfiguration>> GetTenantEmailConfiguration()
        {
            var tenantId = _currentUser.OrganizationId;
            var tenantEmailConfigResult = await _tenantEmailConfigurationRepo.GetByTenantIdAsync(tenantId);
            if (tenantEmailConfigResult.IsFailure)
            {
                _logger.LogError("Failed to retrieve email configuration for tenant with id {TenantId}. Error: {Error}", tenantId, tenantEmailConfigResult.Error);
                return Result.Failure<TenantEmailConfiguration>(tenantEmailConfigResult.Error);
            }
            return tenantEmailConfigResult.Value;
        }
    }
}
