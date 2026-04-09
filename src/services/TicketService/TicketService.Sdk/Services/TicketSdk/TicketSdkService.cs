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

        public async Task<Result<int>> CreateTicketAsync(string title, Guid? customerId, Guid? assignedUserId, CancellationToken ct = default)
        {
            try
            {
                var stream = await Client.Ticket.Create.PostAsync(new CreateTicketRequest
                {
                    Title = title,
                    CustomerId = customerId,
                    AssignedUserId = assignedUserId
                }, cancellationToken: ct);

                if (stream is null)
                    return Result.Failure<int>(Error.Failure("TicketCreationFailed", "No response received."));

                using var reader = new System.IO.StreamReader(stream);
                var body = await reader.ReadToEndAsync(ct);
                if (int.TryParse(body.Trim(), out var ticketId))
                    return Result.Success(ticketId);

                return Result.Failure<int>(Error.Failure("TicketCreationFailed", "Could not parse ticket ID from response."));
            }
            catch (ApiException ex)
            {
                return ex.ResponseStatusCode switch
                {
                    400 => Result.Failure<int>(Error.Validation("BadRequest", "Invalid ticket data.")),
                    _ => Result.Failure<int>(Error.Failure("TicketCreationFailed", $"Failed to create ticket. Status: {ex.ResponseStatusCode}"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ticket");
                return Result.Failure<int>(Error.Failure("TicketCreationFailed", "An error occurred while creating the ticket."));
            }
        }

        private static TicketSdkDto MapToDto(TicketDto ticket)
        {
            var assignedUserId = ticket.Assignment?.AssignmentDto?.UserId;
            var assignedByUserId = ticket.Assignment?.AssignmentDto?.AssignedByUserId;

            if (assignedUserId is null)
            {
                var fallbackData = ticket.Assignment?.TicketDtoAssignmentMember1?.AdditionalData;
                if (fallbackData is { Count: > 0 })
                {
                    if (fallbackData.TryGetValue("userId", out var rawUserId))
                    {
                        var userIdStr = rawUserId?.ToString();
                        if (Guid.TryParse(userIdStr, out var uid))
                            assignedUserId = uid;
                    }
                    if (fallbackData.TryGetValue("assignedByUserId", out var rawAssignedBy))
                    {
                        var assignedByStr = rawAssignedBy?.ToString();
                        if (Guid.TryParse(assignedByStr, out var abid))
                            assignedByUserId = abid;
                    }
                }
            }

            return new TicketSdkDto
            {
                Id = ticket.Id ?? 0,
                Title = ticket.Title ?? string.Empty,
                Status = ticket.Status ?? 0,
                OrganizationId = ticket.OrganizationId ?? Guid.Empty,
                CustomerId = ticket.CustomerId,
                CreatedAt = ticket.CreatedAt,
                AssignedUserId = assignedUserId,
                AssignedByUserId = assignedByUserId
            };
        }
    }
}
