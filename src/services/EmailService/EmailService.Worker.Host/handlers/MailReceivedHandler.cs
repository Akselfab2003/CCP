using CCP.Shared.AuthContext;
using CCP.Shared.Events;
using CCP.Shared.ResultAbstraction;
using CustomerService.Sdk.Services;
using EmailService.Domain.Interfaces;
using EmailService.Domain.Models;
using EmailService.Worker.Host.Services;
using MessagingService.Sdk.Services;
using MimeKit;
using TicketService.Sdk.Services.Ticket;

namespace EmailService.Worker.Host.handlers
{
    public class MailReceivedHandler
    {
        private readonly ILogger<MailReceivedHandler> _logger;
        private readonly IMailBoxService _mailBoxService;
        private readonly IMailManagementController _mailManagementController;
        private readonly IEmailTicketMessageRepository _emailTicketMessageRepository;
        private readonly ICustomerSdkService _customerSdkService;
        private readonly IMessageSdkService _messageSdkService;
        private readonly ITicketService _ticketService;
        private readonly ICurrentUser _currentUser;
        public MailReceivedHandler(ILogger<MailReceivedHandler> logger,
                                   IMailBoxService mailBoxService,
                                   ICustomerSdkService customerSdkService,
                                   IMailManagementController mailManagementController,
                                   IEmailTicketMessageRepository emailTicketMessageRepository,
                                   IMessageSdkService messageSdkService,
                                   ITicketService ticketService,
                                   ICurrentUser currentUser)
        {
            _logger = logger;
            _mailBoxService = mailBoxService;
            _customerSdkService = customerSdkService;
            _mailManagementController = mailManagementController;
            _emailTicketMessageRepository = emailTicketMessageRepository;
            _messageSdkService = messageSdkService;
            _ticketService = ticketService;
            _currentUser = currentUser;
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

            var inReplyTo = fullEmail.InReplyTo;
            var messageId = fullEmail.MessageId;
            var senderEmail = fullEmail.From.Mailboxes.First().Address;
            var senderName = fullEmail.From.Mailboxes.First().Name ?? senderEmail.Split("@").First();

            int TicketId = 0;
            Guid CustomerId = Guid.Empty;


            if (!string.IsNullOrEmpty(inReplyTo))
            {
                _logger.LogInformation("Email is a reply. In-Reply-To: {InReplyTo}", inReplyTo);

                var QueryOriginalEmailResult = await _emailTicketMessageRepository.GetByMessageIdAsync(messageId: inReplyTo);

                if (QueryOriginalEmailResult.IsFailure)
                {
                    _logger.LogWarning("Failed to find original email for In-Reply-To: {InReplyTo}. MessageId: {MessageId}. Subject: {Subject}",
                        inReplyTo, messageId, mail_Received.Subject);

                    var CreatedNewTicketEmail = await NewEmailReceivedFlow(senderEmail, senderName, mail_Received.Subject, EmailTenantDetails.Value.OrganizationId);

                    if (CreatedNewTicketEmail.IsFailure)
                    {
                        _logger.LogError("Failed to create new ticket for email from {From} with subject {Subject} after failing to find original email for In-Reply-To: {InReplyTo}",
                            mail_Received.MailFrom, mail_Received.Subject, inReplyTo);
                        return;
                    }
                    (TicketId, CustomerId) = CreatedNewTicketEmail.Value;

                }
                else
                {
                    EmailTicketMessage originalEmail = QueryOriginalEmailResult.Value;
                    _logger.LogInformation("Found original email for In-Reply-To: {InReplyTo}. Original Sender: {OriginalSender}. Original Subject: {OriginalSubject}",
                        inReplyTo, originalEmail.SenderEmail, originalEmail.Subject);

                    CustomerId = originalEmail.CustomerId;
                    TicketId = originalEmail.TicketId;

                    // Send message to messaging service for it to be processed and linked to the correct ticket based on the original email details
                }

            }
            else
            {
                // No In-Reply-To header, treat as new email and create a new ticket and customer if needed
                var CreatedNewTicketEmail = await NewEmailReceivedFlow(senderEmail, senderName, mail_Received.Subject, EmailTenantDetails.Value.OrganizationId);

                if (CreatedNewTicketEmail.IsFailure)
                {
                    _logger.LogError("Failed to create new ticket for email from {From} with subject {Subject}", mail_Received.MailFrom, mail_Received.Subject);
                    return;
                }
                (TicketId, CustomerId) = CreatedNewTicketEmail.Value;
            }




        }

        public async Task<Result<(int TicketId, Guid CustomerId)>> NewEmailReceivedFlow(string SenderEmail, string SenderName, string Subject, Guid TenantId)
        {
            try
            {
                Guid CustomerId = Guid.Empty;
                var FindCustomerByEmailResult = await _customerSdkService.GetCustomerByEmail(SenderEmail);
                if (FindCustomerByEmailResult.IsFailure)
                {
                    CustomerId = Guid.NewGuid();
                    await _customerSdkService.CreateCustomer(new CustomerService.Sdk.Models.CreateCustomerRequest { Id = CustomerId, Email = SenderEmail, Name = SenderName, OrganizationId = TenantId });
                }

                _currentUser.SetOrganizationId(TenantId);

                var CreateTicketResult = await _ticketService.CreateTicket(new TicketService.Sdk.Dtos.CreateTicketRequestDto()
                {
                    Title = Subject,
                    CustomerId = CustomerId,
                    AssignedUserId = null
                });

                if (CreateTicketResult.IsFailure)
                {
                    return Result.Failure<(int, Guid)>(Error.Failure(code: "TicketCreationFailed", description: "Failed to create a ticket for the incoming email."));
                }

                return Result.Success((CreateTicketResult.Value, CustomerId));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the incoming email from {SenderEmail} with subject {Subject}", SenderEmail, Subject);
                return Result.Failure<(int, Guid)>(Error.Failure(code: "EmailProcessingError", description: "An error occurred while processing the incoming email."));
            }
        }

        public async Task<Result> SaveMessageAsync(MimeMessage message, int ticketId, Guid customerId, Guid TenantId)
        {
            try
            {
                ArgumentException.ThrowIfNullOrEmpty(message.MessageId, nameof(message.MessageId));

                var emailTicketMessage = new EmailTicketMessage()
                {
                    Id = Guid.NewGuid(),
                    TicketId = ticketId,
                    CustomerId = customerId,
                    OrganizationId = TenantId,
                    MessageId = message.MessageId,
                    InReplyTo = message.InReplyTo ?? string.Empty,
                    References = message.References?.ToList() ?? [],
                    SenderEmail = message.From.Mailboxes.First().Address,
                    SenderName = message.From.Mailboxes.First().Name ?? message.From.Mailboxes.First().Address.Split("@").First(),
                    Subject = message.Subject ?? string.Empty,
                    Body = message.TextBody ?? message.HtmlBody ?? string.Empty,
                    SentAt = message.Date.UtcDateTime,
                    Direction = EmailDirection.Inbound
                };
                return await _emailTicketMessageRepository.AddAsync(emailTicketMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save email message for ticket #{TicketId} and customer {CustomerId}", ticketId, customerId);
                return Result.Failure(Error.Failure(code: "EmailMessageSaveError", description: "An error occurred while saving the email message."));
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
