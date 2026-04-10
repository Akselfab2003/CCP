using MailKit;
using MailKit.Net.Imap;

namespace EmailService.Worker.Host.Services
{
    public class ImapMailReciver
    {
        private readonly string hostUrl;
        private readonly IConfiguration configuration;
        private readonly int port = 993;

        public ImapMailReciver(string hostUrl, IConfiguration configuration)
        {
            this.hostUrl = hostUrl;
            this.configuration = configuration;
        }
        public async Task ListenerAsync()
        {
            using var client = new ImapClient();
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            await client.ConnectAsync(hostUrl, port, true);
            await client.AuthenticateAsync(configuration.GetValue<string>("emailWorkerServiceUsername"), configuration.GetValue<string>("emailWorkerServicePassword"));

            await client.Inbox.OpenAsync(MailKit.FolderAccess.ReadOnly);
            client.Inbox.CountChanged += async (sender, e) =>
            {
                var inbox = sender as IMailFolder;
                var mails = await inbox.FetchAsync(0, -1, MailKit.MessageSummaryItems.Full | MailKit.MessageSummaryItems.UniqueId);
                foreach (var item in mails)
                {
                    Console.WriteLine(item.Body);
                    Console.WriteLine(item.Date);
                    Console.WriteLine(item.NormalizedSubject);
                }
            };
            await Task.Delay(-1);
        }

        public async Task ConnectAsync()
        {
            using var client = new ImapClient();
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            await client.ConnectAsync(hostUrl, port, true);
            await client.AuthenticateAsync(configuration.GetValue<string>("emailWorkerServiceUsername"), configuration.GetValue<string>("emailWorkerServicePassword"));

            var inbox = client.Inbox;
            inbox.Open(MailKit.FolderAccess.ReadOnly);

            Console.WriteLine("Total messages: {0}", inbox.Count);
            Console.WriteLine("Recent messages: {0}", inbox.Recent);

            var mails = await inbox.FetchAsync(0, -1, MailKit.MessageSummaryItems.Full | MailKit.MessageSummaryItems.UniqueId);

            foreach (var item in mails)
            {
                Console.WriteLine(item.Body);
                Console.WriteLine(item.Date);
                Console.WriteLine(item.NormalizedSubject);
            }

        }
    }
}
