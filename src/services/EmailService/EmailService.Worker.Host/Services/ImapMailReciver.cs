using MailKit;
using MailKit.Net.Imap;
using MimeKit;

namespace EmailService.Worker.Host.Services
{
    public class ImapMailReciver
    {
        private readonly ILogger<ImapMailReciver> _logger;
        private readonly IConfiguration _configuration;
        private readonly int _port = 993;

        public ImapMailReciver(ILogger<ImapMailReciver> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task ListenerAsync(CancellationToken cancellationToken = default)
        {
            var hostUrl = _configuration.GetValue<string>("emailWorkerServiceHostUrl")
                ?? throw new InvalidOperationException("emailWorkerServiceHostUrl configuration is required.");

            var userName = _configuration.GetValue<string>("emailWorkerServiceUsername")
                ?? throw new InvalidOperationException("emailWorkerServiceUsername configuration is required.");

            var password = _configuration.GetValue<string>("emailWorkerServicePassword")
                ?? throw new InvalidOperationException("emailWorkerServicePassword configuration is required.");


            using var client = new ImapClient();
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            try
            {
                await client.ConnectAsync(hostUrl, _port, true, cancellationToken);
                await client.AuthenticateAsync(userName, password, cancellationToken);

                var inbox = client.Inbox ?? throw new InvalidOperationException("Inbox folder not found.");

                await client.Inbox.OpenAsync(MailKit.FolderAccess.ReadOnly, cancellationToken);

                // Set up event handler for new emails
                client.Inbox.CountChanged += async (sender, e) =>
                {
                    await HandleNewEmailsAsync(sender as IMailFolder, cancellationToken);
                };

                _logger.LogInformation("IMAP listener started successfully at {time}", DateTimeOffset.Now);

                // Keep the listener running
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
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
            catch (OperationCanceledException)
            {
                _logger.LogInformation("IMAP listener was cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in IMAP listener.");
                throw;
            }
            finally
            {
                if (client.IsConnected)
                {
                    await client.DisconnectAsync(true, cancellationToken);
                    _logger.LogInformation("Disconnected from IMAP server.");
                }
            }
        }

        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            var hostUrl = _configuration.GetValue<string>("emailWorkerServiceHostUrl")
                ?? throw new InvalidOperationException("emailWorkerServiceHostUrl configuration is required.");

            var userName = _configuration.GetValue<string>("emailWorkerServiceUsername")
                ?? throw new InvalidOperationException("emailWorkerServiceUsername configuration is required.");

            var password = _configuration.GetValue<string>("emailWorkerServicePassword")
                ?? throw new InvalidOperationException("emailWorkerServicePassword configuration is required.");

            using var client = new ImapClient();
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            try
            {
                await client.ConnectAsync(hostUrl, _port, true, cancellationToken);
                await client.AuthenticateAsync(userName, password, cancellationToken);

                var inbox = client.Inbox ?? throw new InvalidOperationException("Inbox folder not found.");
                await inbox.OpenAsync(MailKit.FolderAccess.ReadOnly, cancellationToken);

                _logger.LogInformation("Total messages: {count}", inbox.Count);
                _logger.LogInformation("Recent messages: {recent}", inbox.Recent);

                var mails = await inbox.FetchAsync(0, -1,
                    MailKit.MessageSummaryItems.Full | MailKit.MessageSummaryItems.UniqueId,
                    cancellationToken);

                foreach (var item in mails)
                {
                    _logger.LogInformation("Mail received - Subject: {subject}, Date: {date}, From: {from}",
                        item.NormalizedSubject ?? "No subject",
                        item.Date,
                        item.EmailId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error connecting to mailbox.");
                throw;
            }
            finally
            {
                if (client.IsConnected)
                {
                    await client.DisconnectAsync(true, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Handles new emails received in the inbox
        /// </summary>
        private async Task HandleNewEmailsAsync(IMailFolder? inbox, CancellationToken cancellationToken)
        {
            if (inbox == null)
            {
                _logger.LogWarning("Inbox is null, cannot handle new emails.");
                return;
            }

            try
            {
                var mails = await inbox.FetchAsync(0, -1,
                    MailKit.MessageSummaryItems.Full | MailKit.MessageSummaryItems.UniqueId,
                    cancellationToken);

                foreach (var item in mails)
                {
                    try
                    {
                        _logger.LogInformation("Processing email - Subject: {subject}, From: {from}",
                            item.NormalizedSubject ?? "No subject",
                            item.EmailId);

                        // TODO: Extract ticket ID from email subject or headers
                        // Example: "[Ticket #123]" or "RE: Support Request - Ticket #123"
                        var ticketId = ExtractTicketIdFromSubject(item.NormalizedSubject);

                        if (ticketId.HasValue)
                        {
                            _logger.LogInformation("Email is a reply to Ticket #{ticketId}", ticketId);

                            // TODO: Call EmailSendingService to notify support team
                            // await _emailSdkService.NotifyTicketReplyAsync(
                            //     customerId, ticketId, customerEmail, agentName, agentRole, replyContent);
                        }
                        else
                        {
                            _logger.LogInformation("Email is not a ticket reply, treating as new ticket");

                            // TODO: Create new ticket from this email
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing individual email with subject: {subject}",
                            item.NormalizedSubject ?? "Unknown");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching new emails from inbox.");
            }
        }

        /// <summary>
        /// Extracts ticket ID from email subject line
        /// Looks for patterns like "[Ticket #123]" or "Ticket #123"
        /// </summary>
        private int? ExtractTicketIdFromSubject(string? subject)
        {
            if (string.IsNullOrWhiteSpace(subject))
                return null;

            // Pattern: "[Ticket #123]" or "Ticket #123"
            var patterns = new[]
            {
                @"\[?[Tt]icket\s+#(\d+)\]?",  // Matches: [Ticket #123] or Ticket #123
                @"#(\d+)"                       // Matches: #123
            };

            foreach (var pattern in patterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(subject, pattern);
                if (match.Success && int.TryParse(match.Groups[1].Value, out int ticketId))
                {
                    return ticketId;
                }
            }

            return null;
        }
    }
}
