using EmailService.Sdk.Services;
using Microsoft.Extensions.Logging;
using TicketService.Application.Services.Assignment;
using TicketService.Domain.Entities;
using TicketService.Domain.Interfaces;
using TicketService.Domain.RequestObjects;

namespace TicketService.Application.Services.Ticket
{
    public class TicketCommands : ITicketCommands
    {
        private readonly ILogger<TicketCommands> _logger;
        private readonly ITicketRepositoryCommands _ticketRepository;
        private readonly IAssignmentCommands _assignmentCommands;
        private readonly ICurrentUser _currentUser;
        private readonly IEmailSdkService _emailSdkService;
        private readonly ITicketHistoryRepository _historyRepository;


        public TicketCommands(ILogger<TicketCommands> logger, ITicketRepositoryCommands ticketRepository, ICurrentUser currentUser, IAssignmentCommands assignmentCommands, IEmailSdkService emailSdkService, ITicketHistoryRepository historyRepository)
        {
            _logger = logger;
            _ticketRepository = ticketRepository;
            _currentUser = currentUser;
            _assignmentCommands = assignmentCommands;
            _emailSdkService = emailSdkService;
            _historyRepository = historyRepository;
        }

        public async Task<Result<int>> CreateTicketAsync(CreateTicketRequest request)
        {
            try
            {
                var ticket = new Domain.Entities.Ticket();
                ticket.AddRequiredInfo(request.Title, request.CustomerId, _currentUser.OrganizationId);
                Result<Domain.Entities.Ticket> result = await _ticketRepository.AddAsync(ticket);

                await _ticketRepository.SaveChangesAsync();

                if (result.IsFailure)
                {
                    _logger.LogError("Failed to create ticket: {Error}", result.Error);
                    return Result.Failure<int>(result.Error);
                }

                if (request.AssignedUserId != null)
                {
                    var assignmentResult = await _assignmentCommands.CreateAssignmentAsync(result.Value.Id, request.AssignedUserId.Value);
                    if (assignmentResult.IsFailure)
                    {
                        _logger.LogError("Failed to create assignment for ticket {TicketId}: {Error}", result.Value.Id, assignmentResult.Error);
                        return Result.Failure<int>(assignmentResult.Error);
                    }

                    ticket.UpdateAssignmentReference(assignmentResult.Value);
                }

                await _ticketRepository.SaveChangesAsync();

                try
                {
                    if (request.CustomerId.HasValue && request.CustomerId.Value != Guid.Empty)
                        await _emailSdkService.NotifyTicketCreatedAsync(request.CustomerId.Value, result.Value.Title, result.Value.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send ticket creation email for ticket {TicketId}, but ticket was created successfully", result.Value.Id);
                }

                return Result.Success(result.Value.Id); // ← return the ticket ID
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the ticket.");
                return Result.Failure<int>(Error.Failure(code: "TicketCreationFailed", description: "An error occurred while creating the ticket."));
            }
        }

        public async Task<Result> UpdateTicketStatusAsync(int ticketId, TicketStatus newStatus)
        {
            try
            {
                var result = await _ticketRepository.UpdateStatusAsync(ticketId, newStatus);
                if (result.IsFailure)
                {
                    _logger.LogError("Failed to update status for ticket {TicketId}: {Error}", ticketId, result.Error);
                    return result;
                }

                await _historyRepository.AddAsync(TicketHistoryEntry.Create(
                    ticketId,
                    actorUserId: null,
                    eventType: "StatusChanged",
                    oldValue: null,
                    newValue: newStatus.ToString()
                ));

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating status for ticket {TicketId}.", ticketId);
                return Result.Failure(Error.Failure("StatusUpdateFailed", "An error occurred while updating the ticket status."));
            }
        }

        public async Task<Result> RecordMessageSentAsync(int ticketId, Guid? senderUserId, string messageSnippet)
        {
            try
            {
                var snippet = messageSnippet.Length > 120 ? messageSnippet[..120] : messageSnippet;
                await _historyRepository.AddAsync(TicketHistoryEntry.Create(
                    ticketId,
                    actorUserId: senderUserId,
                    eventType: "MessageSent",
                    oldValue: null,
                    newValue: snippet
                ));
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while recording message history for ticket {TicketId}.", ticketId);
                return Result.Failure(Error.Failure("RecordMessageFailed", "An error occurred while recording message history."));
            }
        }
    }
}
