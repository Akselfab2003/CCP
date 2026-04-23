using System.Net.Http.Json;
using Gateway.Sdk.Dtos;
using Microsoft.Extensions.Logging;

namespace Gateway.Sdk.Services
{
    internal class GatewayApiClientService : IGatewayService
    {
        private const string ClientName = "GatewayServiceClient";

        private readonly ILogger<GatewayApiClientService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public GatewayApiClientService(ILogger<GatewayApiClientService> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Result<TicketDetailAggregateSdkDto>> GetTicketDetailAsync(int ticketId, CancellationToken ct = default)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient(ClientName);
                var response = await httpClient.GetAsync($"/gateway/tickets/{ticketId}/detail", ct);

                if (!response.IsSuccessStatusCode)
                {
                    return response.StatusCode switch
                    {
                        System.Net.HttpStatusCode.NotFound => Result.Failure<TicketDetailAggregateSdkDto>(Error.NotFound("NotFound", $"Ticket {ticketId} was not found.")),
                        System.Net.HttpStatusCode.Unauthorized => Result.Failure<TicketDetailAggregateSdkDto>(Error.Failure("Unauthorized", "You are not authorized.")),
                        System.Net.HttpStatusCode.Forbidden => Result.Failure<TicketDetailAggregateSdkDto>(Error.Failure("Forbidden", "You do not have permission.")),
                        _ => Result.Failure<TicketDetailAggregateSdkDto>(Error.Failure("GatewayError", $"Gateway returned status {(int)response.StatusCode}."))
                    };
                }

                var dto = await response.Content.ReadFromJsonAsync<TicketDetailAggregateSdkDto>(cancellationToken: ct);
                return dto is not null
                    ? Result.Success(dto)
                    : Result.Failure<TicketDetailAggregateSdkDto>(Error.Failure("GatewayError", "Gateway returned an empty response."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching ticket detail for ticket {TicketId}", ticketId);
                return Result.Failure<TicketDetailAggregateSdkDto>(Error.Failure("GatewayError", "An error occurred while fetching ticket detail."));
            }
        }

        public async Task<Result<ManagerDashboardAggregateSdkDto>> GetManagerDashboardAsync(CancellationToken ct = default)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient(ClientName);
                var response = await httpClient.GetAsync("/gateway/dashboard/manager", ct);

                if (!response.IsSuccessStatusCode)
                {
                    return response.StatusCode switch
                    {
                        System.Net.HttpStatusCode.Unauthorized => Result.Failure<ManagerDashboardAggregateSdkDto>(Error.Failure("Unauthorized", "You are not authorized.")),
                        System.Net.HttpStatusCode.Forbidden => Result.Failure<ManagerDashboardAggregateSdkDto>(Error.Failure("Forbidden", "You do not have permission.")),
                        _ => Result.Failure<ManagerDashboardAggregateSdkDto>(Error.Failure("GatewayError", $"Gateway returned status {(int)response.StatusCode}."))
                    };
                }

                var dto = await response.Content.ReadFromJsonAsync<ManagerDashboardAggregateSdkDto>(cancellationToken: ct);
                return dto is not null
                    ? Result.Success(dto)
                    : Result.Failure<ManagerDashboardAggregateSdkDto>(Error.Failure("GatewayError", "Gateway returned an empty response."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching manager dashboard");
                return Result.Failure<ManagerDashboardAggregateSdkDto>(Error.Failure("GatewayError", "An error occurred while fetching manager dashboard."));
            }
        }
    }
}
