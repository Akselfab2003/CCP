using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;
using TicketService.Sdk.Dtos;
using TicketService.Sdk.Models;

namespace TicketService.Sdk.Services.TicketSdk
{
    internal class TicketSdkService : ITicketSdkService
    {
        private readonly IKiotaApiClient<TicketServiceClient> _client;
        private readonly ILogger<TicketSdkService> _logger;

        private TicketServiceClient Client => _client.Client;

        public TicketSdkService(IKiotaApiClient<TicketServiceClient> client, ILogger<TicketSdkService> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<Result<TicketSdkDto>> GetTicketAsync(int ticketId, CancellationToken ct = default)
        {
            try
            {
                var ticket = await Client.Ticket.GetTicket[ticketId].GetAsync(cancellationToken: ct);

                if (ticket is null)
                    return Result.Failure<TicketSdkDto>(Error.NotFound("TicketNotFound", $"Ticket with id {ticketId} not found."));

                return Result.Success(MapToDto(ticket));
            }
            catch (ApiException ex)
            {
                return ex.ResponseStatusCode switch
                {
                    404 => Result.Failure<TicketSdkDto>(Error.NotFound("TicketNotFound", $"Ticket with id {ticketId} not found.")),
                    _ => Result.Failure<TicketSdkDto>(Error.Failure("TicketRetrievalFailed", $"Failed to retrieve ticket. Status: {ex.ResponseStatusCode}"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ticket {TicketId}", ticketId);
                return Result.Failure<TicketSdkDto>(Error.Failure("TicketRetrievalFailed", "An error occurred while retrieving the ticket."));
            }
        }

        public async Task<Result<List<TicketSdkDto>>> GetTicketsAsync(
            Guid? assignedUserId = null,
            Guid? customerId = null,
            string? status = null,
            CancellationToken ct = default)
        {
            try
            {
                var tickets = await Client.Ticket.GetTickets.GetAsync(req =>
                {
                    req.QueryParameters = new()
                    {
                        AssignedUserId = assignedUserId,
                        CustomerId = customerId,
                        TicketStatus = status
                    };
                }, cancellationToken: ct);

                if (tickets is null)
                    return Result.Success(new List<TicketSdkDto>());

                return Result.Success(tickets.Select(MapToDto).ToList());
            }
            catch (ApiException ex)
            {
                return Result.Failure<List<TicketSdkDto>>(Error.Failure("TicketRetrievalFailed", $"API error. Status: {ex.ResponseStatusCode}. Message: {ex.Message}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tickets");
                return Result.Failure<List<TicketSdkDto>>(Error.Failure("TicketRetrievalFailed", $"Exception: {ex.GetType().Name}: {ex.Message}"));
            }
        }

        public async Task<Result> CreateTicketAsync(string title, Guid? customerId, Guid? assignedUserId, CancellationToken ct = default)
        {
            try
            {
                await Client.Ticket.Create.PostAsync(new CreateTicketRequest
                {
                    Title = title,
                    CustomerId = customerId,
                    AssignedUserId = assignedUserId
                }, cancellationToken: ct);

                return Result.Success();
            }
            catch (ApiException ex)
            {
                return ex.ResponseStatusCode switch
                {
                    400 => Result.Failure(Error.Validation("BadRequest", "Invalid ticket data.")),
                    _ => Result.Failure(Error.Failure("TicketCreationFailed", $"Failed to create ticket. Status: {ex.ResponseStatusCode}"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ticket");
                return Result.Failure(Error.Failure("TicketCreationFailed", "An error occurred while creating the ticket."));
            }
        }

        private static TicketSdkDto MapToDto(TicketDto ticket)
        {
            return new TicketSdkDto
            {
                Id = ticket.Id ?? 0,
                Title = ticket.Title ?? string.Empty,
                Status = ticket.Status ?? 0,
                OrganizationId = ticket.OrganizationId ?? Guid.Empty,
                CustomerId = ticket.CustomerId,
                CreatedAt = ticket.CreatedAt,
                AssignedUserId = ticket.Assignment?.UserId,
                AssignedByUserId = ticket.Assignment?.AssignedByUserId
            };
        }
    }
}
