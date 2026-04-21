using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;
using System.Net.Http.Json;
using TicketService.Sdk.Dtos;
using TicketService.Sdk.Mappers;

namespace TicketService.Sdk.Services.Ticket
{
    internal class TicketApiClientService : ITicketService
    {
        private const string ClientName = "TicketServiceClient";

        private readonly ILogger<TicketApiClientService> _logger;
        private readonly IKiotaApiClient<TicketServiceClient> _client;
        private readonly IHttpClientFactory _httpClientFactory;

        private TicketServiceClient Client => _client.Client;

        public TicketApiClientService(ILogger<TicketApiClientService> logger, IKiotaApiClient<TicketServiceClient> client, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _client = client;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Result<int>> CreateTicket(CreateTicketRequestDto request, CancellationToken ct = default)
        {
            try
            {
                var result = await Client.Ticket.Create.PostAsync(new Models.CreateTicketRequest()
                {
                    Title = request.Title,
                    CustomerId = request.CustomerId,
                    AssignedUserId = request.AssignedUserId
                }, cancellationToken: ct);
                return result;
            }
            catch (ApiException ex)
            {
                return ex.ResponseStatusCode switch
                {
                    400 => Result.Failure<int>(Error.Failure(code: "BadRequest", description: "The request was invalid. Please check the provided data.")),
                    401 => Result.Failure<int>(Error.Failure(code: "Unauthorized", description: "You are not authorized to perform this action.")),
                    403 => Result.Failure<int>(Error.Failure(code: "Forbidden", description: "You do not have permission to perform this action.")),
                    404 => Result.Failure<int>(Error.Failure(code: "NotFound", description: "The specified resource was not found.")),
                    _ => Result.Failure<int>(Error.Failure(code: "TicketCreationFailed", description: $"An error occurred while creating the ticket. Status code: {ex.ResponseStatusCode}"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the ticket.");
                return Result.Failure<int>(Error.Failure(code: "TicketCreationFailed", description: "An error occurred while creating the ticket."));
            }
        }

        public async Task<Result<TicketSdkDto>> GetTicket(int ticketId, CancellationToken ct = default)
        {
            try
            {
                var ticket = await Client.Ticket.GetTicket[ticketId].GetAsync(cancellationToken: ct);

                return ticket is not null
                    ? Result.Success(ticket.ToDto())
                    : Result.Failure<TicketSdkDto>(Error.Failure(code: "NotFound", description: $"No ticket found with id {ticketId}."));
            }
            catch (ApiException ex)
            {
                return ex.ResponseStatusCode switch
                {
                    400 => Result.Failure<TicketSdkDto>(Error.Failure(code: "BadRequest", description: "The request was invalid. Please check the provided parameters.")),
                    401 => Result.Failure<TicketSdkDto>(Error.Failure(code: "Unauthorized", description: "You are not authorized to perform this action.")),
                    403 => Result.Failure<TicketSdkDto>(Error.Failure(code: "Forbidden", description: "You do not have permission to perform this action.")),
                    404 => Result.Failure<TicketSdkDto>(Error.Failure(code: "NotFound", description: $"No ticket found with id {ticketId}.")),
                    _ => Result.Failure<TicketSdkDto>(Error.Failure(code: "TicketRetrievalFailed", description: $"An error occurred while retrieving the ticket with id {ticketId}. Status code: {ex.ResponseStatusCode}"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the ticket with id {TicketId}.", ticketId);
                return Result.Failure<TicketSdkDto>(Error.Failure(code: "TicketRetrievalFailed", description: $"An error occurred while retrieving the ticket with id {ticketId}."));
            }
        }

        public async Task<Result<List<TicketSdkDto>>> GetTickets(Guid? assignedUserId = null,
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
                    ? Result.Success(tickets.ToDto())
                    : Result.Failure<List<TicketSdkDto>>(Error.Failure(code: "NotFound", description: "No tickets found with the provided parameters."));
            }
            catch (ApiException ex)
            {
                return ex.ResponseStatusCode switch
                {
                    400 => Result.Failure<List<TicketSdkDto>>(Error.Failure(code: "BadRequest", description: "The request was invalid. Please check the provided parameters.")),
                    401 => Result.Failure<List<TicketSdkDto>>(Error.Failure(code: "Unauthorized", description: "You are not authorized to perform this action.")),
                    403 => Result.Failure<List<TicketSdkDto>>(Error.Failure(code: "Forbidden", description: "You do not have permission to perform this action.")),
                    _ => Result.Failure<List<TicketSdkDto>>(Error.Failure(code: "TicketRetrievalFailed", description: $"An error occurred while retrieving tickets. Status code: {ex.ResponseStatusCode}"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving tickets.");
                return Result.Failure<List<TicketSdkDto>>(Error.Failure(code: "TicketRetrievalFailed", description: "An error occurred while retrieving tickets."));
            }
        }


        public async Task<Result> UpdateTicketStatusAsync(int ticketId, TicketStatus newStatus, CancellationToken ct = default)
        {
            try
            {
                await Client.Ticket[ticketId].Status.PatchAsync(new Models.UpdateTicketStatusRequest()
                {
                    NewStatus = (int)newStatus
                }, cancellationToken: ct);

                return Result.Success();
            }
            catch (ApiException ex)
            {
                return ex.ResponseStatusCode switch
                {
                    400 => Result.Failure(Error.Failure(code: "BadRequest", description: "The request was invalid. Please check the provided parameters.")),
                    401 => Result.Failure(Error.Failure(code: "Unauthorized", description: "You are not authorized to perform this action.")),
                    403 => Result.Failure(Error.Failure(code: "Forbidden", description: "You do not have permission to perform this action.")),
                    404 => Result.Failure(Error.Failure(code: "NotFound", description: $"No ticket found with id {ticketId}.")),
                    _ => Result.Failure(Error.Failure(code: "TicketStatusUpdateFailed", description: $"An error occurred while updating the status of ticket with id {ticketId}. Status code: {ex.ResponseStatusCode}"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the status of ticket with id {TicketId}.", ticketId);
                return Result.Failure(Error.Failure(code: "TicketStatusUpdateFailed", description: $"An error occurred while updating the status of ticket with id {ticketId}."));
            }
        }

        public async Task<Result<List<TicketHistoryEntryDto>>> GetCustomerHistoryAsync(Guid customerId, int limit = 20, CancellationToken ct = default)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient(ClientName);
                var response = await httpClient.GetAsync($"/ticket/history/customer/{customerId}?limit={limit}", ct);

                if (!response.IsSuccessStatusCode)
                    return Result.Failure<List<TicketHistoryEntryDto>>(Error.Failure(
                        code: "HistoryRetrievalFailed",
                        description: $"Failed to retrieve customer history. Status code: {(int)response.StatusCode}"));

                var entries = await response.Content.ReadFromJsonAsync<List<TicketHistoryEntryDto>>(cancellationToken: ct);
                return Result.Success(entries ?? new List<TicketHistoryEntryDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving history for customer {CustomerId}.", customerId);
                return Result.Failure<List<TicketHistoryEntryDto>>(Error.Failure(
                    code: "HistoryRetrievalFailed",
                    description: "An error occurred while retrieving customer history."));
            }
        }

        public async Task<Result<ManagerStatsSdkDto>> GetManagerStatsAsync(CancellationToken ct = default)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient(ClientName);
                var response = await httpClient.GetAsync("/ticket/manager-stats", ct);

                if (!response.IsSuccessStatusCode)
                    return Result.Failure<ManagerStatsSdkDto>(Error.Failure(
                        code: "ManagerStatsRetrievalFailed",
                        description: $"Failed to retrieve manager stats. Status code: {(int)response.StatusCode}"));

                var stats = await response.Content.ReadFromJsonAsync<ManagerStatsSdkDto>(cancellationToken: ct);
                return Result.Success(stats ?? new ManagerStatsSdkDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving manager stats.");
                return Result.Failure<ManagerStatsSdkDto>(Error.Failure(
                    code: "ManagerStatsRetrievalFailed",
                    description: "An error occurred while retrieving manager stats."));
            }
        }

        public async Task<Result> RecordMessageSentAsync(int ticketId, Guid? senderUserId, string messageSnippet, CancellationToken ct = default)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient(ClientName);
                var response = await httpClient.PostAsJsonAsync(
                    $"/ticket/{ticketId}/history/message",
                    new { SenderUserId = senderUserId, MessageSnippet = messageSnippet },
                    ct);

                if (!response.IsSuccessStatusCode)
                    return Result.Failure(Error.Failure(
                        code: "RecordMessageFailed",
                        description: $"Failed to record message history. Status code: {(int)response.StatusCode}"));

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while recording message history for ticket {TicketId}.", ticketId);
                return Result.Failure(Error.Failure(
                    code: "RecordMessageFailed",
                    description: "An error occurred while recording message history."));
            }
        }

        public async Task<Result<List<TicketHistoryEntryDto>>> GetOrgHistoryAsync(int limit = 20, CancellationToken ct = default)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient(ClientName);
                var response = await httpClient.GetAsync($"/ticket/history/org?limit={limit}", ct);

                if (!response.IsSuccessStatusCode)
                    return Result.Failure<List<TicketHistoryEntryDto>>(Error.Failure(
                        code: "HistoryRetrievalFailed",
                        description: $"Failed to retrieve org history. Status code: {(int)response.StatusCode}"));

                var entries = await response.Content.ReadFromJsonAsync<List<TicketHistoryEntryDto>>(cancellationToken: ct);
                return Result.Success(entries ?? new List<TicketHistoryEntryDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving org history.");
                return Result.Failure<List<TicketHistoryEntryDto>>(Error.Failure(
                    code: "HistoryRetrievalFailed",
                    description: "An error occurred while retrieving org history."));
            }
        }
        public async Task<Result<List<TicketHistoryEntryDto>>> GetMyHistoryAsync(int limit = 20, CancellationToken ct = default)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient(ClientName);
                var response = await httpClient.GetAsync($"/ticket/history/mine?limit={limit}", ct);

                if (!response.IsSuccessStatusCode)
                    return Result.Failure<List<TicketHistoryEntryDto>>(Error.Failure(
                        code: "HistoryRetrievalFailed",
                        description: $"Failed to retrieve assigned ticket history. Status code: {(int)response.StatusCode}"));

                var entries = await response.Content.ReadFromJsonAsync<List<TicketHistoryEntryDto>>(cancellationToken: ct);
                return Result.Success(entries ?? new List<TicketHistoryEntryDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving assigned ticket history.");
                return Result.Failure<List<TicketHistoryEntryDto>>(Error.Failure(
                    code: "HistoryRetrievalFailed",
                    description: "An error occurred while retrieving assigned ticket history."));
            }
        }
    }
}
