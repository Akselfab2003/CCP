using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TicketService.Domain.Entities;
using TicketService.Domain.Interfaces;

namespace TicketService.Infrastructure.Persistence.Repositories
{
    public class AssignmentRepository : IAssignmentRepository
    {
        private readonly ILogger<AssignmentRepository> _logger;
        private readonly TicketDbContext _context;

        public AssignmentRepository(ILogger<AssignmentRepository> logger, TicketDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<Result<Assignment>> AddAsync(Assignment assignment)
        {
            try
            {
                var assignmentEntity = await _context.Assignments.AddAsync(assignment);
                return assignmentEntity.Entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the assignment.");
                return Result.Failure<Assignment>(Error.Failure(code: "AssignmentCreationFailed", description: "An error occurred while creating the assignment."));
            }
        }

        public async Task<Result<Assignment>> GetByIdAsync(Guid id)
        {
            try
            {
                var assignment = await _context.Assignments.FindAsync(id);

                if (assignment == null)
                {
                    return Result.Failure<Assignment>(Error.Failure(code: "AssignmentNotFound", description: $"No assignment found with id {id}."));
                }

                return Result.Success(assignment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the assignment with id {AssignmentId}.", id);
                return Result.Failure<Assignment>(Error.Failure(code: "AssignmentRetrievalFailed", description: $"An error occurred while retrieving the assignment with id {id}."));
            }
        }

        public async Task<Result<Assignment>> UpdateAsync(Assignment assignment)
        {
            try
            {
                var existingAssignment = await _context.Assignments.FindAsync(assignment.Id);
                if (existingAssignment == null)
                {
                    return Result.Failure<Assignment>(Error.Failure(code: "AssignmentNotFound", description: $"No assignment found with id {assignment.Id}."));
                }
                _context.Entry(existingAssignment).CurrentValues.SetValues(assignment);
                return existingAssignment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the assignment with id {AssignmentId}.", assignment.Id);
                return Result.Failure<Assignment>(Error.Failure(code: "AssignmentUpdateFailed", description: $"An error occurred while updating the assignment with id {assignment.Id}."));
            }
        }

        public async Task<Result<Assignment>> GetAssignmentByTicketIdAsync(int ticketId)
        {
            try
            {
                var assignment = await _context.Assignments.SingleOrDefaultAsync(a => a.TicketId == ticketId);
                if (assignment == null)
                    return Result.Failure<Assignment>(Error.Failure(code: "AssignmentNotFound", description: $"No assignment found for ticket with id {ticketId}."));

                return Result.Success(assignment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving assignments for ticket with id {TicketId}.", ticketId);
                return Result.Failure<Assignment>(Error.Failure(code: "AssignmentsRetrievalFailed", description: $"An error occurred while retrieving assignments for ticket with id {ticketId}."));
            }
        }


        public Task SaveChangesAsync()
            => _context.SaveChangesAsync();
    }
}
