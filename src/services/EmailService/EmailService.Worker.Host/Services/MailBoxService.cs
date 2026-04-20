using MailKit;
using MailKit.Net.Imap;

namespace EmailService.Worker.Host.Services
{
    public class MailBoxService
    {
        private readonly ILogger<MailBoxService> _logger;
        private readonly IConfiguration _configuration;

        public MailBoxService(ILogger<MailBoxService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task DetectNewMailsAsync(int count, string fullFolderName)
        {
            // Validate configuration values
            var hostUrl = _configuration.GetValue<string>("emailWorkerServiceHostUrl")
                ?? throw new InvalidOperationException("emailWorkerServiceHostUrl configuration is required.");

            var userName = _configuration.GetValue<string>("emailWorkerServiceUsername")
                ?? throw new InvalidOperationException("emailWorkerServiceUsername configuration is required.");

            var password = _configuration.GetValue<string>("emailWorkerServicePassword")
                ?? throw new InvalidOperationException("emailWorkerServicePassword configuration is required.");

            using var client = new ImapClient();

            try
            {
                // Connect and authenticate
                await client.ConnectAsync(hostUrl, 143, false);
                await client.AuthenticateAsync(userName, password);

                // Get the target folder
                var folders = await client.GetFoldersAsync(client.PersonalNamespaces[0]);
                var folder = folders.FirstOrDefault(f => f.FullName == fullFolderName);

                if (folder == null)
                {
                    _logger.LogWarning("Folder '{folderName}' not found in mailbox.", fullFolderName);
                    return;
                }

                // Fetch the latest mail(s)
                var mails = await folder.FetchAsync(count - 1, count - 1,
                    MailKit.MessageSummaryItems.Full | MailKit.MessageSummaryItems.UniqueId);

                if (mails.Count == 0)
                {
                    _logger.LogInformation("No new mails found at: {time}", DateTimeOffset.Now);
                    return;
                }

                // TODO: Check if the mail is a reply to an existing ticket email or a new email 
                // without a ticket and save it to the database and handle it accordingly

                foreach (var item in mails)
                {
                    _logger.LogInformation("New mail received at: {time}", DateTimeOffset.Now);
                    _logger.LogInformation("Mail subject: {subject}", item.NormalizedSubject ?? "No subject");
                    _logger.LogInformation("Mail date: {date}", item.Date);
                    _logger.LogInformation("Mail from: {from}", item.EmailId);
                }
            }
            catch (MailKit.Security.AuthenticationException ex)
            {
                _logger.LogError(ex, "Failed to authenticate with email server. Check credentials.");
                throw;
            }
            catch (MailKit.ProtocolException ex)
            {
                _logger.LogError(ex, "IMAP protocol error while connecting to email server.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while detecting new mails.");
                throw;
            }
            finally
            {
                if (client.IsConnected)
                {
                    await client.DisconnectAsync(true);
                }
            }
        }
    }
}
