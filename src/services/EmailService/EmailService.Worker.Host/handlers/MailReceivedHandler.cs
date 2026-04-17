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

        // Keywords that suggest a customer wants support
        private static readonly string[] SupportKeywords =
        [
            "help", "issue", "problem", "broken", "error", "not working",
            "support", "request", "bug", "crash", "fail", "trouble",
            "urgent", "cannot", "can't", "unable", "wrong", "stuck"
        ];

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
                _logger.LogInformation(
                    "Handled mail_received event: Subject: {Subject}, From: {From}, To: {To}, MessageId: {MessageId}",
                    mail_Received.Subject, mail_Received.MailFrom, mail_Received.MailTo, mail_Received.MessageId);

                var ticketId = ExtractTicketIdFromSubject(mail_Received.Subject);

                if (ticketId != null)
                {
                    // Known ticket — notify support that customer replied
                    await SendSupportCustomerReplyEmailAsync(mail_Received, ticketId.Value);
                }
                else if (LooksSupportRequest(mail_Received.Subject, mail_Received.Body))
                {
                    // No ticket ID but looks like a support request — confirm to customer
                    await SendNewTicketConfirmationAsync(mail_Received);
                }
                else
                {
                    _logger.LogWarning(
                        "Email does not match a ticket reply or support request. Subject: {Subject}. Skipping.",
                        mail_Received.Subject);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling mail_received event");
            }
        }

        private async Task SendSupportCustomerReplyEmailAsync(mail_received mail_Received, int ticketId)
        {
            try
            {
                var organizationName = _configuration.GetValue<string>("EmailSettings:OrganizationName") ?? "Support Team";
                var supportTeamEmail = _configuration.GetValue<string>("emailWorkerServiceUsername") ?? "support@example.com";
                var replyUrl = _configuration.GetValue<string>("ApplicationUrls:TicketReply") ?? "#";
                var managementUrl = _configuration.GetValue<string>("ApplicationUrls:ManageTicket") ?? "#";
                var viewHistoryUrl = _configuration.GetValue<string>("ApplicationUrls:TicketHistory") ?? "#";

                var emailModel = new EmailReceived
                {
                    Id = ticketId,
                    Subject = mail_Received.Subject,
                    Body = mail_Received.Body,
                    SenderAddress = mail_Received.MailFrom,
                    RecipientAddress = supportTeamEmail,
                    ReceivedAt = DateTime.UtcNow,
                };

                await _emailSendingService.SendSupportCustomerReplyEmailAsync(
                    to: supportTeamEmail,
                    subject: mail_Received.Subject,
                    email: emailModel,
                    customerName: ExtractNameFromEmail(mail_Received.MailFrom),
                    customerEmail: mail_Received.MailFrom,
                    organizationName: organizationName,
                    ticketStatus: "open",
                    ticketStatusLabel: "Open",
                    replyUrl: replyUrl,
                    managementUrl: managementUrl,
                    viewHistoryUrl: viewHistoryUrl);

                _logger.LogInformation(
                    "Sent support customer-reply notification for ticket #{TicketId} to {SupportEmail}",
                    ticketId, supportTeamEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send support customer-reply notification for ticket #{TicketId}", ticketId);
            }
        }

        private async Task SendNewTicketConfirmationAsync(mail_received mail_Received)
        {
            try
            {
                var organizationName = _configuration.GetValue<string>("EmailSettings:OrganizationName") ?? "Support Team";
                var expectedResponse = _configuration.GetValue<string>("EmailSettings:ExpectedResponseTime") ?? "24 hours";
                var portalUrl = _configuration.GetValue<string>("ApplicationUrls:CustomerPortal") ?? "#";

                var emailModel = new EmailSent
                {
                    Subject = mail_Received.Subject,
                    Body = mail_Received.Body,
                    SenderAddress = _configuration.GetValue<string>("emailWorkerServiceUsername") ?? "",
                    RecipientAddress = mail_Received.MailFrom,
                    SentAt = DateTime.UtcNow,
                };

                await _emailSendingService.SendTicketCreatedEmailAsync(
                    to: mail_Received.MailFrom,
                    subject: $"[Ticket Created] {mail_Received.Subject}",
                    email: emailModel,
                    organizationName: organizationName,
                    expectedResponseTime: expectedResponse,
                    portalUrl: portalUrl);

                _logger.LogInformation(
                    "Sent new ticket confirmation to {CustomerEmail} for subject: {Subject}",
                    mail_Received.MailFrom, mail_Received.Subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send new ticket confirmation to {CustomerEmail}", mail_Received.MailFrom);
            }
        }

        private static bool LooksSupportRequest(string subject, string body)
        {
            // Check both subject and body against the keyword list (case-insensitive)
            var combined = $"{subject} {body}".ToLowerInvariant();
            return SupportKeywords.Any(keyword => combined.Contains(keyword));
        }

        private int? ExtractTicketIdFromSubject(string subject)
        {
            var patterns = new[] { @"\[?[Tt]icket\s*#?(\d+)\]?", @"\[#(\d+)\]" };

            foreach (var pattern in patterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(subject, pattern);
                if (match.Success && int.TryParse(match.Groups[1].Value, out var ticketId))
                    return ticketId;
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
