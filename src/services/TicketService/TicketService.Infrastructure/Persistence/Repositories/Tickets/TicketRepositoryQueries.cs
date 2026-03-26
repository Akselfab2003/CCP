using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TicketService.Domain.Interfaces;
using TicketService.Domain.ResponseObjects;
using TicketService.Infrastructure.Persistence.Repositories.Tickets.Querying;

namespace TicketService.Infrastructure.Persistence.Repositories.Tickets
{
    public class TicketRepositoryQueries : ITicketRepositoryQueries
    {
        private readonly ILogger<TicketRepositoryCommands> _logger;
        private readonly TicketDbContext _context;

        public TicketRepositoryQueries(ILogger<TicketRepositoryCommands> logger, TicketDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<Result<List<TicketDto>>> GetTicketsBasedOnParameters(Guid? assignedUserId = null, Guid? CustomerId = null, TicketStatus? status = null)
        {
            try
            {
                List<TicketDto> tickets = await _context.Tickets.Query(assignedUserId: assignedUserId, CustomerId: CustomerId, status: status)
                                                                .IncludeAssignment()
                                                                .ToDto()
                                                                .ToListAsync();
                return Result.Success(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving tickets based on the provided parameters.");
                return Result.Failure<List<TicketDto>>(Error.Failure(code: "TicketRetrievalFailed", description: "An error occurred while retrieving tickets based on the provided parameters."));
            }
        }

        public async Task<Result<TicketDto>> GetTicket(int? id, Guid? AssignmentId = null)
        {
            try
            {
                var ticket = await _context.Tickets.Query(id: id, assignedUserId: AssignmentId)
                                                   .IncludeAssignment()
                                                   .ToDto()
                                                   .SingleOrDefaultAsync();
                if (ticket == null)
                {
                    return Result.Failure<TicketDto>(Error.Failure(code: "TicketNotFound", description: $"No ticket found with the provided parameters."));
                }

                return Result.Success(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the ticket with the provided parameters.");
                return Result.Failure<TicketDto>(Error.Failure(code: "TicketRetrievalFailed", description: $"An error occurred while retrieving the ticket with the provided parameters."));
            }
        }

    }
}
