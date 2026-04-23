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
        private readonly ServiceAccountOverrider _serviceAccountOverrider;
        public MailReceivedHandler(ILogger<MailReceivedHandler> logger,
                                   IMailBoxService mailBoxService,
                                   ICustomerSdkService customerSdkService,
                                   IMailManagementController mailManagementController,
                                   IEmailTicketMessageRepository emailTicketMessageRepository,
                                   IMessageSdkService messageSdkService,
                                   ITicketService ticketService,
                                   ServiceAccountOverrider serviceAccountOverrider)
        {
            _logger = logger;
            _mailBoxService = mailBoxService;
            _customerSdkService = customerSdkService;
            _mailManagementController = mailManagementController;
            _emailTicketMessageRepository = emailTicketMessageRepository;
            _messageSdkService = messageSdkService;
            _ticketService = ticketService;
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

            var inReplyTo = fullEmail.InReplyTo;
            var messageId = fullEmail.MessageId;
            var senderEmail = fullEmail.From.Mailboxes.First().Address;
            var senderName = fullEmail.From.Mailboxes.First().Name ?? senderEmail.Split("@").First();
            var content = fullEmail.TextBody ?? fullEmail.HtmlBody ?? string.Empty;

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

                    var CreatedNewTicketEmail = await NewEmailReceivedFlow(senderEmail, senderName, mail_Received.Subject, content, EmailTenantDetails.Value.OrganizationId);

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
                    await _messageSdkService.CreateMessageAsync(ticketId: TicketId,
                                                                organizationId: _serviceAccountOverrider.OrganizationId,
                                                                userId: null,
                                                                content: content,
                                                                isInternalNote: false);
                }
            }
            else
            {
                // No In-Reply-To header, treat as new email and create a new ticket and customer if needed
                var CreatedNewTicketEmail = await NewEmailReceivedFlow(senderEmail, senderName, mail_Received.Subject, content, EmailTenantDetails.Value.OrganizationId);

                if (CreatedNewTicketEmail.IsFailure)
                {
                    _logger.LogError("Failed to create new ticket for email from {From} with subject {Subject}", mail_Received.MailFrom, mail_Received.Subject);
                    return;
                }
                (TicketId, CustomerId) = CreatedNewTicketEmail.Value;
            }

            var saveResult = await SaveMessageAsync(message: fullEmail, ticketId: TicketId, customerId: CustomerId,
                                     TenantId: EmailTenantDetails.Value.OrganizationId);

            if (saveResult.IsFailure)
            {
                _logger.LogError("Failed to save email message for ticket #{TicketId} and customer {CustomerId}. Email Subject: {Subject}",
                    TicketId, CustomerId, mail_Received.Subject);
            }

        }

        public async Task<Result<(int TicketId, Guid CustomerId)>> NewEmailReceivedFlow(string SenderEmail, string SenderName, string Subject, string Content, Guid TenantId)
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

                _serviceAccountOverrider.SetOrganizationId(TenantId);

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

                await _messageSdkService.CreateMessageAsync(ticketId: CreateTicketResult.Value,
                                                              organizationId: _serviceAccountOverrider.OrganizationId,
                                                              userId: null,
                                                              content: Content,
                                                              isInternalNote: false);

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
    }
}
