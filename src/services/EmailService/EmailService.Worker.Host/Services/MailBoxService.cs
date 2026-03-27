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

        public async Task DetectNewMailsAsync(int Count, string FullFolderName)
        {

            using var client = new ImapClient();

            // TODO: Make the username and password dynamic by reading from Database

            await client.ConnectAsync(_configuration.GetValue<string>("emailWorkerServiceHostUrl"), 143, false);
            await client.AuthenticateAsync(_configuration.GetValue<string>("emailWorkerServiceUsername"), _configuration.GetValue<string>("emailWorkerServicePassword"));

            var folders = await client.GetFoldersAsync(client.PersonalNamespaces[0]);
            var folder = folders.FirstOrDefault(f => f.FullName == FullFolderName);

            if (folder == null)
                return;

            IList<IMessageSummary> mails = await folder.FetchAsync(Count - 1, Count - 1, MailKit.MessageSummaryItems.Full | MailKit.MessageSummaryItems.UniqueId);

            // TODO: Check if the mail is a reply to an existing ticket email or a new email without a ticket and save it to the database and handle it accordingly

            foreach (var item in mails)
            {
                _logger.LogInformation("New mail received at: {time}", DateTimeOffset.Now);
                _logger.LogInformation("Mail subject: {subject}", item.NormalizedSubject);
                _logger.LogInformation("Mail date: {date}", item.Date);
            }

        }
    }
}
