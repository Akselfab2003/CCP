using CCP.Shared.AuthContext;
using CCP.Shared.Events;
using CCP.Shared.ResultAbstraction;
using EmailService.Domain.Interfaces;
using EmailService.Domain.Models;
using EmailService.Worker.Host.Services;
using MimeKit;

namespace EmailService.Worker.Host.handlers
{
    public class MailReceivedHandler
    {
        private readonly ILogger<MailReceivedHandler> _logger;
        private readonly IMailBoxService _mailBoxService;
        private readonly IEmailTicketMessageRepository _emailTicketMessageRepository;
        private readonly IMailProcessingService _mailProcessingService;
        private readonly ServiceAccountOverrider _serviceAccountOverrider;

        public MailReceivedHandler(
            ILogger<MailReceivedHandler> logger, IMailBoxService mailBoxService,
            IEmailTicketMessageRepository emailTicketMessageRepository, IMailProcessingService mailProcessingService,
            ServiceAccountOverrider serviceAccountOverrider)
        {
            _logger = logger;
            _mailBoxService = mailBoxService;
            _emailTicketMessageRepository = emailTicketMessageRepository;
            _mailProcessingService = mailProcessingService;
            _serviceAccountOverrider = serviceAccountOverrider;
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
            _serviceAccountOverrider.SetOrganizationId(EmailTenantDetails.Value.OrganizationId);


            Result<MimeMessage> mailResult = await _mailBoxService.GetMailFromMailServer(mail_Received.MessageId, EmailTenantDetails.Value);

            if (mailResult == null || mailResult.IsFailure)
            {
                _logger.LogError("Failed to retrieve full email details from mail server for email from {From}. Subject: {Subject}",
                    mail_Received.MailFrom, mail_Received.Subject);
                return;
            }

            MimeMessage fullEmail = mailResult.Value;

            var ProcessResult = await _mailProcessingService.ProcessIncomingMailAsync(fullEmail);
            if (ProcessResult.IsFailure)
            {
                _logger.LogError("Failed to process incoming email from {From}. Subject: {Subject}. Error: {Error}",
                    mail_Received.MailFrom, mail_Received.Subject, ProcessResult.Error.Description);
                return;
            }

            (int TicketId, Guid CustomerId) = ProcessResult.Value;

            var saveResult = await SaveMessageAsync(
                message: fullEmail, ticketId: TicketId,
                customerId: CustomerId, TenantId: EmailTenantDetails.Value.OrganizationId);

            if (saveResult.IsFailure)
            {
                _logger.LogError("Failed to save email message for ticket #{TicketId} and customer {CustomerId}. Email Subject: {Subject}",
                    TicketId, CustomerId, mail_Received.Subject);
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
    }
}
