using CCP.Shared.Events;
using EmailService.Application.Interfaces;
using EmailService.Domain.Models;

namespace EmailService.Worker.Host.handlers
{
    public class MailReceivedHandler
    {
        private readonly ILogger<MailReceivedHandler> _logger;
        private readonly IEmail _emailSendingService;
        private readonly IConfiguration _configuration;

        public MailReceivedHandler(
            ILogger<MailReceivedHandler> logger,
            IEmail emailSendingService,
            IConfiguration configuration
            )
        {
            _logger = logger;
            _emailSendingService = emailSendingService;
            _configuration = configuration;
        }

        public async Task Handle(mail_received mail_Received)
        {
            try
            {
                _logger.LogInformation($"Handled mail_received event: Subject: {mail_Received.Subject}, From: {mail_Received.MailFrom}, To: {mail_Received.MailTo}, MessageId: {mail_Received.MessageId}");

                var ticketId = ExtractTicketIdFromSubject(mail_Received.Subject);

                if (ticketId == null)
                {
                    await SendTicketReplyEmailAsync(mail_Received, ticketId);
                }
                else
                {
                    _logger.LogWarning($"Could not extract ticket ID from email subject: {mail_Received.Subject}. Skipping ticket reply email.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling mail_received event");
            }

        }

        private async Task SendTicketReplyEmailAsync(mail_received mail_Received, int? ticketId)
        {
            try
            {
                var organizationName = _configuration.GetValue<string>("EmailSettings:OrganizationName") ?? "Support Team";
                var portalUrl = _configuration.GetValue<string>("ApplicationUrls:CustomerPortal") ?? "#";
                var replyUrl = _configuration.GetValue<string>("ApplicationUrls:TicketReply") ?? "#";
                var viewHistoryUrl = _configuration.GetValue<string>("ApplicationUrls:TicketHistory") ?? "#";
                var reopenUrl = _configuration.GetValue<string>("ApplicationUrls:TicketReopen") ?? "#";

                var emailModel = new EmailReceived
                {
                    Subject = mail_Received.Subject,
                    Body = mail_Received.Body,
                    SenderAddress = mail_Received.MailFrom,
                    RecipientAddress = mail_Received.MailTo,
                    ReceivedAt = DateTime.UtcNow,
                };

                await _emailSendingService.SendTicketReplyEmailAsync(
                    to: mail_Received.MailTo,
                    subject: $"[Reply] Ticket #{ticketId}",
                    email: emailModel,
                    recipientName: ExtractNameFromEmail(mail_Received.MailTo),
                    organizationName: organizationName,
                    agentName: "Support Team",
                    agentRole: "Suppport Agent",
                    ticketStatus: "Open",
                    ticketStatusLabel: "Open",
                    replyUrl: replyUrl,
                    portalUrl: portalUrl,
                    viewHistoryUrl: viewHistoryUrl,
                    reopenUrl: reopenUrl
                );
                _logger.LogInformation($"Sent ticket reply email for ticket #{ticketId} to {mail_Received.MailTo}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send ticket reply email for ticket #{ticketId} to {mail_Received.MailTo}");
            }
        }

        private int? ExtractTicketIdFromSubject(string subject)
        {
            var patterns = new[] { @"\[?[Tt]icket\s*#?(\d+)\]?", @"\[#(\d+)\]" };

            foreach (var pattern in patterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(subject, pattern);
                if (match.Success && int.TryParse(match.Groups[1].Value, out var ticketId))
                {
                    return ticketId;
                }
            }

            return null;
        }

        private string ExtractNameFromEmail(string email)
        {
            var parts = email.Split('@');
            return parts.Length > 0 ? parts[0] : email;
        }
    }
}
