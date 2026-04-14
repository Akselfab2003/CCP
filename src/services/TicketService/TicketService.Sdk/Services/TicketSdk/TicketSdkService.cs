using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;
using System.Net.Http;
using System.Net.Http.Json;
using TicketService.Sdk.Dtos;
using TicketService.Sdk.Models;
using TicketService.Sdk.Services.Assignment;

namespace TicketService.Sdk.Services.TicketSdk
{
    internal class TicketSdkService : ITicketSdkService
    {
        private readonly IKiotaApiClient<TicketServiceClient> _client;
        private readonly ILogger<TicketSdkService> _logger;
        private readonly IAssignmentService _assignmentService;
        private readonly IHttpClientFactory _httpClientFactory;

        private TicketServiceClient Client => _client.Client;

        public TicketSdkService(IKiotaApiClient<TicketServiceClient> client, ILogger<TicketSdkService> logger, IAssignmentService assignmentService, IHttpClientFactory httpClientFactory)
        {
            _client = client;
            _logger = logger;
            _assignmentService = assignmentService;
            _httpClientFactory = httpClientFactory;
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

        public async Task<Result> AssignTicketAsync(int ticketId, Guid userId, CancellationToken ct = default)
        {
            try
            {
                await _assignmentService.AssignTicketToUserAsync(ticketId, userId, ct);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning ticket {TicketId} to user {UserId}", ticketId, userId);
                return Result.Failure(Error.Failure("AssignmentFailed", "An error occurred while assigning the ticket."));
            }
        }

        public async Task<Result> UpdateTicketStatusAsync(int ticketId, TicketStatus newStatus, CancellationToken ct = default)
        {
            try
            {
                var http = _httpClientFactory.CreateClient("TicketServiceClient");
                var response = await http.PatchAsJsonAsync(
                    $"/ticket/{ticketId}/status",
                    new { NewStatus = (int)newStatus },
                    ct);

                if (response.IsSuccessStatusCode)
                    return Result.Success();

                return (int)response.StatusCode switch
                {
                    404 => Result.Failure(Error.NotFound("TicketNotFound", $"Ticket {ticketId} not found.")),
                    _ => Result.Failure(Error.Failure("StatusUpdateFailed", $"Failed to update status. Status: {(int)response.StatusCode}"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for ticket {TicketId}", ticketId);
                return Result.Failure(Error.Failure("StatusUpdateFailed", "An error occurred while updating the ticket status."));
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
