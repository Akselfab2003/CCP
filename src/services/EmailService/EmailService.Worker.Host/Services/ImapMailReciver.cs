using System.Text.RegularExpressions;
using EmailService.Sdk.Services;
using MailKit;
using MailKit.Net.Imap;

namespace EmailService.Worker.Host.Services
{
    public class ImapMailReciver
    {
        private readonly string hostUrl;
        private readonly IConfiguration configuration;
        private readonly IEmailSdkService _emailSdkService;
        private readonly ILogger<ImapMailReciver> _logger;
        private readonly int port = 993;

        private static readonly Regex TicketIdRegex = new(@"#(\d+)",RegexOptions.Compiled);

        public ImapMailReciver(
            string hostUrl, IConfiguration configuration,
            IEmailSdkService emailSdkService, ILogger<ImapMailReciver> logger)
        {
            this.hostUrl = hostUrl;
            this.configuration = configuration;
            this._emailSdkService = emailSdkService;
            this._logger = logger;
        }
        public async Task ListenerAsync()
        {
            using var client = new ImapClient();
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            await client.ConnectAsync(hostUrl, port, true);
            await client.AuthenticateAsync(
                configuration.GetValue<string>("emailWorkerServiceUsername"),
                configuration.GetValue<string>("emailWorkerServicePassword"));

            await client.Inbox.OpenAsync(MailKit.FolderAccess.ReadOnly);

            var knownCount = client.Inbox.Count;

            client.Inbox.CountChanged += async (sender, e) =>
            {
                try
                {
                    var inbox = (IMailFolder)sender!;
                    var currentCount = inbox.Count;

                    if (currentCount <= knownCount)
                        return;

                    var newMessages = await inbox.FetchAsync(
                        knownCount,
                        currentCount -1,
                        MessageSummaryItems.Full | MessageSummaryItems.UniqueId);



                    knownCount = currentCount;

                    foreach ( var message in newMessages)
                    {

                        var fullMessage = await inbox.GetMessageAsync(message.UniqueId);
                        var replyContent = fullMessage.TextBody ?? fullMessage.HtmlBody ?? string.Empty;
                        var fromAddress = fullMessage.From.Mailboxes.FirstOrDefault()?.Address ?? string.Empty;

                        var subject = message.Envelope.Subject ?? string.Empty;
                        var senderAddress = message.Envelope.From.Mailboxes.FirstOrDefault()?.Address ?? string.Empty;

                        _logger.LogInformation("New email received. Subject: {Subject}, Sender: {Sender}", subject, senderAddress);

                        var match = TicketIdRegex.Match(subject);
                        if (!match.Success)
                        {
                            _logger.LogInformation("Email subject does not contain a ticket ID, skipping.");
                            continue;
                        }

                        var ticketId = int.Parse(match.Groups[1].Value);
                        var supportEmail = configuration.GetValue<string>("emailWorkerServiceUsername") ?? "support@example.com";
                        var organizationName = configuration.GetValue<string>("EmailSettings:OrganizationName") ?? "Support Team";

                        _logger.LogInformation("Ticket reply detected for #{TicketId} from {Sender}", ticketId, senderAddress);

                        await _emailSdkService.NotifySupportCustomerReplyAsync(
                            customerId: Guid.Empty,   // see note below
                            agentEmail: supportEmail,
                            agentName: organizationName,
                            ticketId: ticketId,
                            ticketTitle: subject,
                            ticketStatus: "open",
                            ticketStatusLabel: "Open",
                            replyContent: replyContent); 

                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing email.");
                }
            };
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
