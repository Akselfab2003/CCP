using Microsoft.Extensions.Logging;
using TicketService.Domain.Interfaces;
using TicketService.Domain.ResponseObjects;

namespace TicketService.Application.Services.Ticket
{
    public class TicketQueries : ITicketQueries
    {
        private readonly ILogger<TicketQueries> _logger;
        private readonly ITicketRepositoryQueries _ticketRepositoryQueries;

        public TicketQueries(ILogger<TicketQueries> logger, ITicketRepositoryQueries ticketRepositoryQueries)
        {
            _logger = logger;
            _ticketRepositoryQueries = ticketRepositoryQueries;
        }

        public async Task<Result<TicketDto>> GetTicket(int ticketId)
        {
            try
            {
                var ticket = await _ticketRepositoryQueries.GetTicket(ticketId);

                if (ticket.IsFailure)
                    return Result.Failure<TicketDto>(ticket.Error);

                return ticket;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the ticket with id {TicketId}.", ticketId);
                return Result.Failure<TicketDto>(Error.Failure(code: "TicketRetrievalFailed", description: $"An error occurred while retrieving the ticket with id {ticketId}."));
            }
        }

        public async Task<Result<List<TicketDto>>> GetTicketsBasedOnParameters(Guid? assignedUserId = null, Guid? CustomerId = null, TicketStatus? status = null)
        {
            try
            {
                Result<List<TicketDto>> tickets = await _ticketRepositoryQueries.GetTicketsBasedOnParameters(assignedUserId: assignedUserId,
                                                                                                             CustomerId: CustomerId,
                                                                                                             status: status);
                return tickets;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving tickets based on the provided parameters.");
                return Result.Failure<List<TicketDto>>(Error.Failure(code: "TicketRetrievalFailed", description: "An error occurred while retrieving tickets based on the provided parameters."));
            }
        }
    }
}
