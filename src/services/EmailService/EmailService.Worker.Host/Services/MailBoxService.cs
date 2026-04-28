using CCP.Shared.Events;
using CCP.Shared.ResultAbstraction;
using EmailService.Domain.Interfaces;
using EmailService.Domain.Models;
using MailKit.Search;
using MimeKit;

namespace EmailService.Worker.Host.Services
{
    public class MailBoxService : IMailBoxService
    {
        private readonly ILogger<MailBoxService> _logger;
        private readonly IEmailWorkerConfigurationRepo _emailWorkerConfigurationRepo;
        private readonly string _emailhostUrl;

        public MailBoxService(ILogger<MailBoxService> logger, IEmailWorkerConfigurationRepo emailWorkerConfigurationRepo, string emailhostUrl)
        {
            _logger = logger;
            _emailWorkerConfigurationRepo = emailWorkerConfigurationRepo;
            _emailhostUrl = emailhostUrl;
        }

        public async Task<Result<TenantEmailConfiguration>> GetTenantMailboxDetails(mail_received mail_Received)
        {
            try
            {
                var tenantEmailConfig = await _emailWorkerConfigurationRepo.GetByInternalEmailAddress(mail_Received.MailTo);
                if (tenantEmailConfig == null || tenantEmailConfig.IsFailure)
                {
                    _logger.LogWarning("No tenant email configuration found for organizationId: {OrganizationId}", mail_Received.MailTo);
                    return Result.Failure<TenantEmailConfiguration>(Error.NotFound(code: "NotFound", description: $"No tenant email configuration found for email address: {mail_Received.MailTo}"));
                }

                return Result.Success(tenantEmailConfig.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching tenant mailbox details for organizationId: {OrganizationId}", mail_Received.MailTo);
                return Result.Failure<TenantEmailConfiguration>(Error.Failure(code: "FetchError", description: $"An error occurred while fetching tenant mailbox details: {ex.Message}"));
            }
        }

        public async Task<Result<MimeMessage>> GetMailFromMailServer(string MessageId, TenantEmailConfiguration tenantEmailConfiguration)
        {
            try
            {
                using var client = new MailKit.Net.Imap.ImapClient();
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await client.ConnectAsync(_emailhostUrl, 993, true);
                await client.AuthenticateAsync(tenantEmailConfiguration.InternalEmail, tenantEmailConfiguration.InternalEmailPassword);

                if (client.Inbox == null)
                {
                    _logger.LogWarning("Inbox folder not found for email address: {Email}", tenantEmailConfiguration.InternalEmail);
                    return Result.Failure<MimeMessage>(Error.NotFound(code: "InboxNotFound", description: $"Inbox folder not found for email address: {tenantEmailConfiguration.InternalEmail}"));
                }

                await client.Inbox.OpenAsync(MailKit.FolderAccess.ReadOnly);

                var message = await client.Inbox.SearchAsync(SearchQuery.HeaderContains("Message-ID", MessageId));
                if (message == null)
                    return Result.Failure<MimeMessage>(Error.NotFound(code: "MessageNotFound", description: $"No email found with MessageId: {MessageId} in the mailbox"));

                var mail = await client.Inbox.GetMessageAsync(message.FirstOrDefault(), CancellationToken.None);

                if (mail == null)
                    return Result.Failure<MimeMessage>(Error.NotFound(code: "EmailNotFound", description: $"Email with MessageId: {MessageId} not found in the mailbox"));

                await client.DisconnectAsync(true);


                return Result.Success<MimeMessage>(mail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching mail from mail server for email: {Email}", tenantEmailConfiguration.InternalEmail);
                return Result.Failure<MimeMessage>(Error.Failure(code: "MailFetchError", description: $"An error occurred while fetching mail from mail server: {ex.Message}"));
            }
        }
    }
}
