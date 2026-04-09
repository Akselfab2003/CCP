using Microsoft.Extensions.Logging;
using TicketService.Application.Services.Assignment;
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
        public TicketCommands(ILogger<TicketCommands> logger, ITicketRepositoryCommands ticketRepository, ICurrentUser currentUser, IAssignmentCommands assignmentCommands)
        {
            _logger = logger;
            _ticketRepository = ticketRepository;
            _currentUser = currentUser;
            _assignmentCommands = assignmentCommands;
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

                return Result.Success(result.Value.Id); // ← return the ticket ID
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the ticket.");
                return Result.Failure<int>(Error.Failure(code: "TicketCreationFailed", description: "An error occurred while creating the ticket."));
            }
        }
    }
}
