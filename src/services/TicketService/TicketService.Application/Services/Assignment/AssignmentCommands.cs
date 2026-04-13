using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using TicketService.Domain.Interfaces;

namespace TicketService.Application.Services.Assignment
{
    public class AssignmentCommands : IAssignmentCommands
    {
        private readonly ILogger<AssignmentCommands> _logger;
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly ITicketRepositoryCommands _ticketRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IHttpClientFactory _httpClientFactory;

        public AssignmentCommands(
            ILogger<AssignmentCommands> logger,
            IAssignmentRepository assignmentRepository,
            ITicketRepositoryCommands ticketRepository,
            ICurrentUser currentUser,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _assignmentRepository = assignmentRepository;
            _ticketRepository = ticketRepository;
            _currentUser = currentUser;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Result<Guid>> CreateAssignmentAsync(int ticketId, Guid AssignUserId)
        {
            try
            {
                var assignment = new Domain.Entities.Assignment();
                assignment.AddRequiredInfo(ticketId, AssignUserId, _currentUser.UserId);
                var result = await _assignmentRepository.AddAsync(assignment);

                if (result.IsFailure)
                {
                    _logger.LogError("Failed to create assignment: {Error}", result.Error);
                    return Result.Failure<Guid>(result.Error);
                }

                await _assignmentRepository.SaveChangesAsync();
                return Result.Success(result.Value.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the assignment.");
                return Result.Failure<Guid>(Error.Failure(code: "AssignmentCreationFailed", description: "An error occurred while creating the assignment."));
            }
        }

        public async Task<Result<Guid>> CreateOrUpdateAssignment(int ticketId, Guid assignUserId)
        {
            try
            {
                Result<Domain.Entities.Assignment> assignment = await _assignmentRepository.GetAssignmentByTicketIdAsync(ticketId);

                Result<Guid> result;
                if (assignment.IsFailure)
                {
                    _logger.LogInformation("No existing assignment found for ticket {TicketId}. Creating a new assignment.", ticketId);
                    result = await CreateAssignmentAsync(ticketId, assignUserId);
                }
                else
                {
                    Domain.Entities.Assignment existingAssignment = assignment.Value;
                    existingAssignment.UpdateAssignment(assignUserId, _currentUser.UserId);

                    Result<Domain.Entities.Assignment> updateResult = await _assignmentRepository.UpdateAsync(existingAssignment);

                    if (updateResult.IsFailure)
                    {
                        _logger.LogError("Failed to update assignment for ticket {TicketId}: {Error}", ticketId, updateResult.Error);
                        return Result.Failure<Guid>(updateResult.Error);
                    }

                    await _assignmentRepository.SaveChangesAsync();
                    result = Result.Success(existingAssignment.Id);
                }

                if (result.IsSuccess)
                {
                    var ticketResult = await _ticketRepository.GetTicket(ticketId);
                    if (ticketResult.IsSuccess)
                    {
                        ticketResult.Value.UpdateAssignmentReference(result.Value);
                        await _ticketRepository.SaveChangesAsync();
                    }
                    else
                    {
                        _logger.LogWarning("Assignment saved but could not update AssignmentId on ticket {TicketId}: {Error}", ticketId, ticketResult.Error);
                    }

                    await NotifyAssignmentAsync(ticketId, assignUserId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating or updating the assignment.");
                return Result.Failure<Guid>(Error.Failure(code: "AssignmentCreationOrUpdateFailed", description: "An error occurred while creating or updating the assignment."));
            }
        }

        private async Task NotifyAssignmentAsync(int ticketId, Guid assignedUserId)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("MessagingService");
                await client.PostAsJsonAsync("api/ticket-notifications/assignment-updated", new { ticketId, assignedUserId });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to notify MessagingService of assignment update for ticket {TicketId}", ticketId);
            }
        }
    }
}
