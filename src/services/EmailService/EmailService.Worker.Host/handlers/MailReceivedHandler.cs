using CCP.Shared.Events;
using CCP.Shared.ResultAbstraction;
using CustomerService.Sdk.Services;
using EmailService.Domain.Models;
using EmailService.Worker.Host.Services;
using MimeKit;

namespace EmailService.Worker.Host.handlers
{
    public class MailReceivedHandler
    {
        private readonly ILogger<MailReceivedHandler> _logger;
        private readonly IMailBoxService _mailBoxService;
        private readonly IMailManagementController _mailManagementController;
        private readonly ICustomerSdkService _customerSdkService;
        public MailReceivedHandler(ILogger<MailReceivedHandler> logger, IMailBoxService mailBoxService, ICustomerSdkService customerSdkService, IMailManagementController mailManagementController)
        {
            _logger = logger;
            _mailBoxService = mailBoxService;
            _customerSdkService = customerSdkService;
            _mailManagementController = mailManagementController;
        }

        public async Task Handle(mail_received mail_Received)
        {
            _logger.LogInformation(
                "Handled mail_received event: Subject: {Subject}, From: {From}, To: {To}, MessageId: {MessageId}",
                mail_Received.Subject, mail_Received.MailFrom, mail_Received.MailTo, mail_Received.MessageId);

            // Identify tenant based on recipient email address
            Result<TenantEmailConfiguration> EmailTenantDetails = await _mailBoxService.GetTenantMailboxDetails(mail_Received);

            if (EmailTenantDetails == null || EmailTenantDetails.IsFailure)
            {
                _logger.LogError("Failed to retrieve tenant email configuration for incoming email from {From}. Subject: {Subject}",
                    mail_Received.MailFrom, mail_Received.Subject);
                return;
            }

            Result<MimeMessage> mailResult = await _mailBoxService.GetMailFromMailServer(mail_Received.MessageId, EmailTenantDetails.Value);

            if (mailResult == null || mailResult.IsFailure)
            {
                _logger.LogError("Failed to retrieve full email details from mail server for email from {From}. Subject: {Subject}",
                    mail_Received.MailFrom, mail_Received.Subject);
                return;
            }

            MimeMessage fullEmail = mailResult.Value;

            var ExtractCustomerEmailResult = await _mailManagementController.GetCustomerEmailFromMail(fullEmail, EmailTenantDetails.Value);
            if (ExtractCustomerEmailResult.IsFailure)
            {
                _logger.LogError("Failed to extract customer email from incoming email from {From}. Subject: {Subject}",
                    mail_Received.MailFrom, mail_Received.Subject);
                return;
            }

            var customerResult = await _customerSdkService.GetCustomerByEmail(mail_Received.MailFrom);
            if (customerResult.IsFailure)
            {
                _logger.LogWarning("No customer found with email {Email}. Subject: {Subject}. Creating a new customer record.", mail_Received.MailFrom, mail_Received.Subject);

                try
                {
                    await _customerSdkService.CreateCustomer(
                        new CustomerService.Sdk.Models.CreateCustomerRequest
                        {
                            Id = Guid.NewGuid(),
                            Email = ExtractCustomerEmailResult.Value,
                            Name = mail_Received.MailFrom.Split("@").First(),
                            OrganizationId = EmailTenantDetails.Value.OrganizationId
                        });


                    _logger.LogInformation("New customer created from email: {Email}", mail_Received.MailFrom);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to create customer from email: {Email}. Continuing with email processing.", mail_Received.MailFrom);
                }
            }




        }

        #region Old implementation with email processing logic (commented out for now)

        //public async Task Handle(mail_received mail_Received)
        //{
        //    try
        //    {
        //        _logger.LogInformation(
        //            "Handled mail_received event: Subject: {Subject}, From: {From}, To: {To}, MessageId: {MessageId}",
        //            mail_Received.Subject, mail_Received.MailFrom, mail_Received.MailTo, mail_Received.MessageId);

        //        var ticketId = ExtractTicketIdFromSubject(mail_Received.Subject);

        //        var customerResult = await _customerSdkService.GetCustomerByEmail(mail_Received.MailFrom);
        //        if (!customerResult.IsSuccess)
        //        {
        //            try
        //            {
        //                await _customerSdkService.CreateCustomer(
        //                    new CustomerService.Sdk.Models.CreateCustomerRequest
        //                    {
        //                        Id = Guid.NewGuid(),
        //                        Email = mail_Received.MailFrom,
        //                        Name = ExtractNameFromEmail(mail_Received.MailFrom),
        //                        OrganizationId = Guid.Parse(_configuration.GetValue<string>("OrganizationSettings:DefaultOrganizationId") ?? Guid.Empty.ToString())
        //                    });

        //                SendReplyToEmailAsync(mail_Received, ticketId ?? 0).Wait();

        //                _logger.LogInformation("New customer created from email: {Email}", mail_Received.MailFrom);
        //            }
        //            catch (Exception ex)
        //            {
        //                _logger.LogWarning(ex, "Failed to create customer from email: {Email}. Continuing with email processing.", mail_Received.MailFrom);
        //            }
        //        }

        //        if (ticketId != null)
        //        {
        //            // Known ticket — notify support that customer replied
        //            await SendSupportCustomerReplyEmailAsync(mail_Received, ticketId.Value);
        //        }
        //        else if (LooksSupportRequest(mail_Received.Subject, mail_Received.Body))
        //        {
        //            // No ticket ID but looks like a support request — confirm to customer
        //            await SendNewTicketConfirmationAsync(mail_Received);
        //        }
        //        else
        //        {
        //            _logger.LogWarning(
        //                "Email does not match a ticket reply or support request. Subject: {Subject}. Skipping.",
        //                mail_Received.Subject);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error handling mail_received event");
        //    }
        //}

        //private async Task SendReplyToEmailAsync(mail_received mail_Received, int ticketId)
        //{
        //    try
        //    {
        //        var emailModel = new EmailReceived
        //        {
        //            MailId = mail_Received.MessageId,
        //            Subject = mail_Received.Subject,
        //            Body = mail_Received.Body,
        //            SenderAddress = mail_Received.MailFrom,
        //            RecipientAddress = mail_Received.MailTo,
        //            ReceivedAt = DateTime.UtcNow,
        //        };

        //        // Check if there was a previous email sent for this ticket
        //        EmailSent? emailSentModel = null;
        //        try
        //        {
        //            var previousEmailSent = await _emailSentRepository.GetByTicketIdAsync(ticketId);
        //            if (previousEmailSent != null)
        //            {
        //                emailSentModel = previousEmailSent;
        //                _logger.LogInformation("Found previous email sent for ticket #{TicketId}", ticketId);
        //            }
        //            else
        //            {
        //                _logger.LogInformation("No previous email found for ticket #{TicketId}, using null", ticketId);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogWarning(ex, "Failed to retrieve previous email for ticket #{TicketId}, will use null", ticketId);
        //        }

        //        await _emailSendingService.SendReplyToEmailAsync(
        //            to: mail_Received.MailFrom,
        //            subject: $"Re: {mail_Received.Subject}",
        //            emailReceived: emailModel,
        //            emailSent: emailSentModel,
        //            ticketId: ticketId,
        //            organizationName: _configuration.GetValue<string>("EmailSettings:OrganizationName") ?? "Support Team");

        //        _logger.LogInformation(
        //            "Sent reply-to email for ticket #{TicketId} to {CustomerEmail}",
        //            ticketId, mail_Received.MailFrom);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Failed to send reply-to email for ticket #{TicketId}", ticketId);
        //    }
        //}

        //private async Task SendSupportCustomerReplyEmailAsync(mail_received mail_Received, int ticketId)
        //{
        //    try
        //    {
        //        var organizationName = _configuration.GetValue<string>("EmailSettings:OrganizationName") ?? "Support Team";
        //        var supportTeamEmail = _configuration.GetValue<string>("emailWorkerServiceUsername") ?? "support@example.com";
        //        var replyUrl = _configuration.GetValue<string>("ApplicationUrls:TicketReply") ?? "#";
        //        var managementUrl = "";
        //        var viewHistoryUrl = ""; // Planed

        //        var emailModel = new EmailReceived
        //        {
        //            MailId = mail_Received.MessageId,
        //            Subject = mail_Received.Subject,
        //            Body = mail_Received.Body,
        //            SenderAddress = mail_Received.MailFrom,
        //            RecipientAddress = supportTeamEmail,
        //            ReceivedAt = DateTime.UtcNow,
        //        };

        //        await _emailSendingService.SendSupportCustomerReplyEmailAsync(
        //            to: supportTeamEmail,
        //            subject: mail_Received.Subject,
        //            email: emailModel,
        //            customerName: ExtractNameFromEmail(mail_Received.MailFrom),
        //            customerEmail: mail_Received.MailFrom,
        //            organizationName: organizationName,
        //            ticketStatus: "open",
        //            ticketStatusLabel: "Open",
        //            replyUrl: replyUrl,
        //            managementUrl: managementUrl,
        //            viewHistoryUrl: viewHistoryUrl);

        //        _logger.LogInformation(
        //            "Sent support customer-reply notification for ticket #{TicketId} to {SupportEmail}",
        //            ticketId, supportTeamEmail);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Failed to send support customer-reply notification for ticket #{TicketId}", ticketId);
        //    }
        //}

        //private async Task SendNewTicketConfirmationAsync(mail_received mail_Received)
        //{
        //    try
        //    {
        //        var organizationName = _configuration.GetValue<string>("EmailSettings:OrganizationName") ?? "Support Team";
        //        var expectedResponse = _configuration.GetValue<string>("EmailSettings:ExpectedResponseTime") ?? "24 hours";
        //        var portalUrl = _configuration.GetValue<string>("ApplicationUrls:CustomerPortal") ?? "#";

        //        var emailModel = new EmailSent
        //        {
        //            Subject = mail_Received.Subject,
        //            Body = mail_Received.Body,
        //            SenderAddress = _configuration.GetValue<string>("emailWorkerServiceUsername") ?? "",
        //            RecipientAddress = mail_Received.MailFrom,
        //            SentAt = DateTime.UtcNow,
        //        };

        //        await _emailSendingService.SendTicketCreatedEmailAsync(
        //            to: mail_Received.MailFrom,
        //            subject: $"[Ticket Created] {mail_Received.Subject}",
        //            email: emailModel,
        //            organizationName: organizationName,
        //            expectedResponseTime: expectedResponse,
        //            portalUrl: portalUrl);

        //        _logger.LogInformation(
        //            "Sent new ticket confirmation to {CustomerEmail} for subject: {Subject}",
        //            mail_Received.MailFrom, mail_Received.Subject);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Failed to send new ticket confirmation to {CustomerEmail}", mail_Received.MailFrom);
        //    }
        //}

        //private static bool LooksSupportRequest(string subject, string body)
        //{
        //    // Check both subject and body against the keyword list (case-insensitive)
        //    var combined = $"{subject} {body}".ToLowerInvariant();
        //    return SupportKeywords.Any(keyword => combined.Contains(keyword));
        //}

        //private int? ExtractTicketIdFromSubject(string subject)
        //{
        //    var patterns = new[] { @"\[?[Tt]icket\s*#?(\d+)\]?", @"\[#(\d+)\]" };

        //    foreach (var pattern in patterns)
        //    {
        //        var match = System.Text.RegularExpressions.Regex.Match(subject, pattern);
        //        if (match.Success && int.TryParse(match.Groups[1].Value, out var ticketId))
        //            return ticketId;
        //    }

        //    return null;
        //}

        //private string ExtractNameFromEmail(string email)
        //{
        //    var parts = email.Split('@');
        //    return parts.Length > 0 ? parts[0] : email;
        //}


        #endregion
    }
}
