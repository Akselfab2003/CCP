using CCP.Shared.AuthContext;
using CCP.Shared.ResultAbstraction;
using ChatService.Domain.Dtos;
using ChatService.Infrastructure.LLM.Analysis;
using MessagingService.Sdk.Services;
using Microsoft.Extensions.Logging;
using TicketService.Sdk.Services.Ticket;

namespace ChatService.Application.Services.Automated
{
    public class AutomaticMessageGeneration
    {
        private readonly ILogger<AutomaticMessageGeneration> _logger;
        private readonly IMessageSdkService _messageSdkService;
        private readonly ITicketService _ticketService;
        private readonly ITicketAnalysisService _ticketAnalysisService;
        private readonly ICurrentUser _currentUser;

        public AutomaticMessageGeneration(ILogger<AutomaticMessageGeneration> logger,
                                          IMessageSdkService messageSdkService,
                                          ITicketService ticketService,
                                          ITicketAnalysisService ticketAnalysisService,
                                          ICurrentUser currentUser)
        {
            _logger = logger;
            _messageSdkService = messageSdkService;
            _ticketService = ticketService;
            _ticketAnalysisService = ticketAnalysisService;
            _currentUser = currentUser;
        }

        public async Task<Result<string>> GenerateMessage(int ticketId)
        {
            try
            {
                var NewSupportTicketRequst = new SupportTicket();


                var TicketDetailsResult = await _ticketService.GetTicket(ticketId);
                if (TicketDetailsResult.IsFailure)
                    return Result.Failure<string>(TicketDetailsResult.Error);

                NewSupportTicketRequst.TicketId = TicketDetailsResult.Value.Id;
                NewSupportTicketRequst.Title = TicketDetailsResult.Value.Title;
                NewSupportTicketRequst.Description = TicketDetailsResult.Value.Description ?? string.Empty;


                var messagesResult = await _messageSdkService.GetMessagesByTicketIdAsync(ticketId);

                if (messagesResult.IsFailure)
                    return Result.Failure<string>(Error.Failure("MessageRetrievalError", "Failed to retrieve messages for the ticket."));

                var messages = messagesResult.Value;

                Guid customerId = TicketDetailsResult.Value.CustomerId.HasValue ? TicketDetailsResult.Value.CustomerId.Value : Guid.Empty;

                List<TicketMessage> messagesList = new List<TicketMessage>();

                foreach (var msg in messages.Items)
                {
                    MessageAuthorType authorType = msg.UserId.HasValue && msg.UserId.Value == customerId ? MessageAuthorType.User : MessageAuthorType.Support;
                    var sentAt = msg.UpdatedAtUtc.HasValue ? msg.UpdatedAtUtc.Value : msg.CreatedAtUtc;

                    if (!sentAt.HasValue)
                    {
                        _logger.LogWarning("Message with ID {MessageId} has no valid timestamp. Skipping.", msg.Id);
                        continue; // Skip messages without a valid timestamp
                    }

                    var ticketMessage = new TicketMessage
                    {
                        MessageId = msg.Id,
                        TicketId = msg.TicketId,
                        AuthorType = authorType,
                        Content = msg.Content,
                        SentAt = sentAt.Value.DateTime
                    };
                    messagesList.Add(ticketMessage);
                }

                NewSupportTicketRequst.Messages = messagesList;

                Result<TicketProblemAnalysis> analysisResult = await _ticketAnalysisService.ExtractProblemAsync(ticket: NewSupportTicketRequst);

                if (analysisResult.IsFailure)
                    return Result.Failure<string>(Error.Failure("TicketAnalysisError", "Failed to analyze the ticket for problem extraction."));

                var problemAnalysis = analysisResult.Value;


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating automatic message");
                return Result.Failure<string>(Error.Failure("AutomaticMessageGenerationError", "An error occurred while generating the automatic message."));
            }
        }
    }
}
