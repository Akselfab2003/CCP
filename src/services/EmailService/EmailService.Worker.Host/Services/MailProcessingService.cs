using CCP.Shared.AuthContext;
using CCP.Shared.ResultAbstraction;
using CustomerService.Sdk.Services;
using EmailService.Domain.Interfaces;
using EmailService.Domain.Models;
using MessagingService.Sdk.Services;
using MimeKit;
using TicketService.Sdk.Services.Ticket;

namespace EmailService.Worker.Host.Services
{
    public class MailProcessingService : IMailProcessingService
    {
        private readonly ILogger<MailProcessingService> _logger;
        private readonly IEmailTicketMessageRepository _emailTicketMessageRepository;
        private readonly ICustomerSdkService _customerSdkService;
        private readonly IMessageSdkService _messageSdkService;
        private readonly ITicketService _ticketService;
        private readonly ServiceAccountOverrider _serviceAccountOverrider;

        public MailProcessingService(ILogger<MailProcessingService> logger, IEmailTicketMessageRepository emailTicketMessageRepository, ICustomerSdkService customerSdkService, ServiceAccountOverrider serviceAccountOverrider, IMessageSdkService messageSdkService, ITicketService ticketService)
        {
            _logger = logger;
            _emailTicketMessageRepository = emailTicketMessageRepository;
            _customerSdkService = customerSdkService;
            _serviceAccountOverrider = serviceAccountOverrider;
            _messageSdkService = messageSdkService;
            _ticketService = ticketService;
        }

        public async Task<Result<(int TicketId, Guid CustomerId)>> ProcessIncomingMailAsync(MimeMessage message)
        {
            try
            {
                var inReplyTo = message.InReplyTo;
                var messageId = message.MessageId;
                var senderEmail = message.From.Mailboxes.First().Address;
                var senderName = message.From.Mailboxes.First().Name ?? senderEmail.Split("@").First();
                var content = message.TextBody ?? message.HtmlBody ?? string.Empty;
                var subject = message.Subject ?? string.Empty;
                int TicketId = 0;
                Guid CustomerId = Guid.Empty;


                if (!string.IsNullOrEmpty(inReplyTo))
                {
                    _logger.LogInformation("Email is a reply. In-Reply-To: {InReplyTo}", inReplyTo);

                    var QueryOriginalEmailResult = await _emailTicketMessageRepository.GetByMessageIdAsync(messageId: inReplyTo);

                    if (QueryOriginalEmailResult.IsFailure)
                    {
                        _logger.LogWarning("Failed to find original email for In-Reply-To: {InReplyTo}. MessageId: {MessageId}. Subject: {Subject}",
                            inReplyTo, messageId, subject);

                        var CreatedNewTicketEmail = await NewEmailReceivedFlow(senderEmail, senderName, subject, content, _serviceAccountOverrider.OrganizationId);

                        if (CreatedNewTicketEmail.IsFailure)
                        {
                            _logger.LogError("Failed to create new ticket for email from {From} with subject {Subject} after failing to find original email for In-Reply-To: {InReplyTo}",
                                message.From, subject, inReplyTo);
                            return Result.Failure<(int, Guid)>(Error.Failure("MailProcessingFailed", "Failed to find original email for reply and failed to create new ticket for the incoming email."));
                        }
                        (TicketId, CustomerId) = CreatedNewTicketEmail.Value;

                    }
                    else
                    {
                        EmailTicketMessage originalEmail = QueryOriginalEmailResult.Value;
                        _logger.LogInformation("Found original email for In-Reply-To: {InReplyTo}. Original Sender: {OriginalSender}. Original Subject: {OriginalSubject}", inReplyTo, originalEmail.SenderEmail, originalEmail.Subject);

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
                    var CreatedNewTicketEmail = await NewEmailReceivedFlow(senderEmail, senderName, subject, content, _serviceAccountOverrider.OrganizationId);

                    if (CreatedNewTicketEmail.IsFailure)
                    {
                        _logger.LogError("Failed to create new ticket for email from {From} with subject {Subject}", senderEmail, subject);
                        return Result.Failure<(int, Guid)>(Error.Failure("MailProcessingFailed", "Failed to create new ticket for the incoming email."));
                    }
                    (TicketId, CustomerId) = CreatedNewTicketEmail.Value;
                }

                return Result.Success((TicketId, CustomerId));

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing incoming mail.");
                return Result.Failure<(int, Guid)>(Error.Failure("MailProcessingFailed", "An error occurred while processing incoming mail."));
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
                else
                {
                    CustomerId = FindCustomerByEmailResult.Value.Id;
                }

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
                                                              organizationId: TenantId,
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

    }
}
