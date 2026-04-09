using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;

namespace EmailService.Worker.Host.Services
{
    public class ImapMailReciver(IConfiguration configuration, ILogger<ImapMailReciver> logger) : IInboxListener
    {
        public async Task ListenToAllInboxesAsync(IEnumerable<EmailAccount> accounts, CancellationToken cancellationToken)
        {
            var tasks = accounts.Select(acc => ListenSingleInboxAsync(acc, cancellationToken));
            await Task.WhenAll(tasks);
        }

        public async Task ListenSingleInboxAsync(EmailAccount account, CancellationToken cancellationToken)
        {
            var host = configuration["Mail:Host"] ?? "localhost";
            var port = configuration.GetValue("Mail:Port", 143);
            var useSsl = configuration.GetValue("Mail:UseSsl", false);

            while (!cancellationToken.IsCancellationRequested)
            {
                using var client = new ImapClient();

                await client.ConnectAsync(
                    host,
                    port,
                    useSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable,
                    cancellationToken);

                await client.AuthenticateAsync(account.Username, account.Password, cancellationToken);
                await client.Inbox.OpenAsync(FolderAccess.ReadOnly, cancellationToken);

                logger.LogInformation("Connected to {User} at {Host}:{Port}", account.Username, host, port);

                var mails = await client.Inbox.FetchAsync(0,-1,
                    MessageSummaryItems.Envelope | MessageSummaryItems.UniqueId,
                    cancellationToken);

                foreach (var item in mails)
                {
                    logger.LogInformation("[{User}] Mail subject: {Subject}",
                        account.Username,
                        item.Envelope?.Subject);
                }

                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }
        }
    }
}
