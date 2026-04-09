using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;
using TicketService.Sdk.Models;

namespace TicketService.Sdk.Services.Ticket
{
    internal class TicketApiClientService : ITicketService
    {
        private readonly ILogger<TicketApiClientService> _logger;
        private readonly IKiotaApiClient<TicketServiceClient> _client;

        private TicketServiceClient Client => _client.Client;

        public TicketApiClientService(ILogger<TicketApiClientService> logger, IKiotaApiClient<TicketServiceClient> client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task<Result> CreateTicket(CreateTicketRequest request, CancellationToken ct = default)
        {
            try
            {
                await Client.Ticket.Create.PostAsync(request, cancellationToken: ct);
                return Result.Success();
            }
            catch (ApiException ex)
            {
                return ex.ResponseStatusCode switch
                {
                    400 => Result.Failure(Error.Failure(code: "BadRequest", description: "The request was invalid. Please check the provided data.")),
                    401 => Result.Failure(Error.Failure(code: "Unauthorized", description: "You are not authorized to perform this action.")),
                    403 => Result.Failure(Error.Failure(code: "Forbidden", description: "You do not have permission to perform this action.")),
                    404 => Result.Failure(Error.Failure(code: "NotFound", description: "The specified resource was not found.")),
                    _ => Result.Failure(Error.Failure(code: "TicketCreationFailed", description: $"An error occurred while creating the ticket. Status code: {ex.ResponseStatusCode}"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the ticket.");
                return Result.Failure(Error.Failure(code: "TicketCreationFailed", description: "An error occurred while creating the ticket."));
            }
        }

        public async Task<Result<TicketDto>> GetTicket(int ticketId, CancellationToken ct = default)
        {
            try
            {
                var ticket = await Client.Ticket.GetTicket[ticketId].GetAsync(cancellationToken: ct);
                return ticket is not null
                    ? Result.Success(ticket)
                    : Result.Failure<TicketDto>(Error.Failure(code: "NotFound", description: $"No ticket found with id {ticketId}."));
            }
            catch (ApiException ex)
            {
                return ex.ResponseStatusCode switch
                {
                    400 => Result.Failure<TicketDto>(Error.Failure(code: "BadRequest", description: "The request was invalid. Please check the provided parameters.")),
                    401 => Result.Failure<TicketDto>(Error.Failure(code: "Unauthorized", description: "You are not authorized to perform this action.")),
                    403 => Result.Failure<TicketDto>(Error.Failure(code: "Forbidden", description: "You do not have permission to perform this action.")),
                    404 => Result.Failure<TicketDto>(Error.Failure(code: "NotFound", description: $"No ticket found with id {ticketId}.")),
                    _ => Result.Failure<TicketDto>(Error.Failure(code: "TicketRetrievalFailed", description: $"An error occurred while retrieving the ticket with id {ticketId}. Status code: {ex.ResponseStatusCode}"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the ticket with id {TicketId}.", ticketId);
                return Result.Failure<TicketDto>(Error.Failure(code: "TicketRetrievalFailed", description: $"An error occurred while retrieving the ticket with id {ticketId}."));
            }
        }

        public async Task<Result<List<TicketDto>>> GetTickets(Guid? assignedUserId = null,
                                                              Guid? CustomerId = null,
                                                              TicketStatus? status = null,
                                                              CancellationToken ct = default)
        {
            try
            {
                var tickets = await Client.Ticket.GetTickets.GetAsync(req =>
                {
                    req.QueryParameters = new()
                    {
                        AssignedUserId = assignedUserId,
                        CustomerId = CustomerId,
                        TicketStatus = status?.ToString(),
                    };
                });

                return tickets is not null
                    ? Result.Success(tickets)
                    : Result.Failure<List<TicketDto>>(Error.Failure(code: "NotFound", description: "No tickets found with the provided parameters."));
            }
            catch (ApiException ex)
            {
                return ex.ResponseStatusCode switch
                {
                    400 => Result.Failure<List<TicketDto>>(Error.Failure(code: "BadRequest", description: "The request was invalid. Please check the provided parameters.")),
                    401 => Result.Failure<List<TicketDto>>(Error.Failure(code: "Unauthorized", description: "You are not authorized to perform this action.")),
                    403 => Result.Failure<List<TicketDto>>(Error.Failure(code: "Forbidden", description: "You do not have permission to perform this action.")),
                    _ => Result.Failure<List<TicketDto>>(Error.Failure(code: "TicketRetrievalFailed", description: $"An error occurred while retrieving tickets. Status code: {ex.ResponseStatusCode}"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving tickets.");
                return Result.Failure<List<TicketDto>>(Error.Failure(code: "TicketRetrievalFailed", description: "An error occurred while retrieving tickets."));
            }
        }
    }
}
