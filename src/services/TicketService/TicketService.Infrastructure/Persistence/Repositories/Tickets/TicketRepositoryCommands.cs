using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TicketService.Domain.Entities;
using TicketService.Domain.Interfaces;
using TicketService.Infrastructure.Persistence.Repositories.Tickets.Querying;

namespace TicketService.Infrastructure.Persistence.Repositories.Tickets
{
    public class TicketRepositoryCommands : ITicketRepositoryCommands
    {
        private readonly ILogger<TicketRepositoryCommands> _logger;
        private readonly TicketDbContext _context;

        public TicketRepositoryCommands(ILogger<TicketRepositoryCommands> logger, TicketDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<Result<Ticket>> AddAsync(Ticket ticket)
        {
            try
            {
                var ticketEntity = await _context.Tickets.AddAsync(ticket);
                return ticketEntity.Entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the ticket.");
                return Result.Failure<Ticket>(Error.Failure(code: "TicketCreationFailed", description: "An error occurred while creating the ticket."));
            }
        }

        public async Task<Result<List<Ticket>>> GetTicketsBasedOnParameters(Guid? assignedUserId = null, Guid? CustomerId = null, TicketStatus? status = null)
        {
            try
            {
                List<Ticket> tickets = await _context.Tickets.Query(assignedUserId: assignedUserId, CustomerId: CustomerId, status: status)
                                                             .ToListAsync();
                return Result.Success(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving tickets based on the provided parameters.");
                return Result.Failure<List<Ticket>>(Error.Failure(code: "TicketRetrievalFailed", description: "An error occurred while retrieving tickets based on the provided parameters."));
            }
        }

        public async Task<Result<Ticket>> GetTicket(int? id, Guid? AssignmentId = null)
        {
            try
            {
                var ticket = await _context.Tickets.Query(id: id, assignedUserId: AssignmentId).FirstOrDefaultAsync();
                if (ticket == null)
                {
                    return Result.Failure<Ticket>(Error.Failure(code: "TicketNotFound", description: $"No ticket found with the provided parameters."));
                }
                return Result.Success(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the ticket with the provided parameters.");
                return Result.Failure<Ticket>(Error.Failure(code: "TicketRetrievalFailed", description: $"An error occurred while retrieving the ticket with the provided parameters."));
            }
        }

        public Task SaveChangesAsync()
            => _context.SaveChangesAsync();

    }
}
