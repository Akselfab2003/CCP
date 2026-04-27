using CCP.Shared.ValueObjects;
using Gateway.Sdk.Dtos;
using Gateway.Sdk.Mappers;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;

namespace Gateway.Sdk.Services
{
    internal class GatewayApiClientService : IGatewayService
    {
        private const string ClientName = "GatewayServiceClient";

        private readonly ILogger<GatewayApiClientService> _logger;
        private readonly IKiotaApiClient<GatewayClient> _apiClient;

        public GatewayApiClientService(ILogger<GatewayApiClientService> logger, IKiotaApiClient<GatewayClient> apiClient)
        {
            _logger = logger;
            _apiClient = apiClient;
        }

        public async Task<Result<Dtos.TicketDetailAggregateSdkDto>> GetTicketDetailAsync(int ticketId, CancellationToken ct = default)
        {
            try
            {
                var dto = await _apiClient.Client.Gateway.Tickets[ticketId].Detail.GetAsync(cancellationToken: ct);
                if (dto is null)
                {
                    return Result.Failure<Dtos.TicketDetailAggregateSdkDto>(Error.Failure("GatewayError", "Gateway returned an empty response."));
                }

                if (dto.Ticket is null || dto.UserNames is null)
                {
                    return Result.Failure<Dtos.TicketDetailAggregateSdkDto>(Error.Failure("GatewayError", "Gateway returned incomplete ticket detail data."));
                }

                TicketService.Sdk.Dtos.TicketSdkDto Ticket = new TicketService.Sdk.Dtos.TicketSdkDto
                {
                    Id = dto.Ticket.Id.HasValue ? dto.Ticket.Id.Value : 0,
                    Title = dto.Ticket.Title ?? string.Empty,
                    Description = dto.Ticket.Description ?? string.Empty,
                    Status = dto.Ticket.Status ?? 0,
                    CreatedAt = dto.Ticket.CreatedAt,
                    OrganizationId = dto.Ticket.OrganizationId ?? Guid.Empty,
                    AssignedByUserId = dto.Ticket.AssignedByUserId,
                    AssignedUserId = dto.Ticket.AssignedUserId,
                    CustomerId = dto.Ticket.CustomerId,
                    Origin = (TicketOrigin)(dto.Ticket.Origin ?? 0)
                };
                var messages = new List<MessagingService.Sdk.Dtos.MessageDto>();
                if (dto.Messages != null)
                {
                    messages = dto.Messages.Select(m => m.ToMessagingServiceDto()).Where(m => m != null).ToList()!;
                }

                var sdkDto = new Dtos.TicketDetailAggregateSdkDto
                {
                    Ticket = Ticket,
                    Messages = messages,
                    UserNames = dto.UserNames switch
                    {
                        IDictionary<string, string> sdict => new Dictionary<string, string>(sdict),
                        IDictionary<string, object> odict => odict.Where(kv => kv.Value is string)
                                                                  .ToDictionary(kv => kv.Key, kv => (string)kv.Value),
                        _ => new Dictionary<string, string>()
                    }
                };


                return dto is not null
                    ? Result.Success(sdkDto)
                    : Result.Failure<Dtos.TicketDetailAggregateSdkDto>(Error.Failure("GatewayError", "Gateway returned an empty response."));
            }
            catch (ApiException ex)
            {
                return ex.ResponseStatusCode switch
                {
                    404 => Result.Failure<TicketDetailAggregateSdkDto>(Error.NotFound("NotFound", $"Ticket {ticketId} was not found.")),
                    401 => Result.Failure<TicketDetailAggregateSdkDto>(Error.Failure("Unauthorized", "You are not authorized.")),
                    403 => Result.Failure<TicketDetailAggregateSdkDto>(Error.Failure("Forbidden", "You do not have permission.")),
                    _ => Result.Failure<TicketDetailAggregateSdkDto>(Error.Failure("GatewayError", $"Gateway returned status {(int)ex.ResponseStatusCode}."))
                };
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
                var response = await _apiClient.Client.Gateway.Dashboard.Manager.GetAsync(cancellationToken: ct);

                if (response is null)
                {
                    return Result.Failure<ManagerDashboardAggregateSdkDto>(Error.Failure("GatewayError", "Gateway returned an empty response."));
                }

                var stats = response.Stats;
                var managerStats = stats != null
                    ? new TicketService.Sdk.Dtos.ManagerStatsSdkDto
                    {
                        AvgResponseTime = stats.AvgResponseTime ?? string.Empty,
                        AwaitingUser = stats.AwaitingUser ?? 0,
                        ClosedToday = stats.ClosedToday ?? 0,
                        OpenTickets = stats.OpenTickets ?? 0,
                        TeamPerformance = stats.TeamPerformance?
                            .Select(tp => new TicketService.Sdk.Dtos.SupporterPerformanceSdkDto
                            {
                                UserId = tp.UserId.HasValue ? tp.UserId.Value : Guid.Empty,
                                ResolvedCount = tp.ResolvedCount.HasValue ? tp.ResolvedCount.Value : 0,
                            }).ToList() ?? new List<TicketService.Sdk.Dtos.SupporterPerformanceSdkDto>()
                    }
                    : null;

                var sdkDto = new ManagerDashboardAggregateSdkDto
                {
                    Stats = managerStats ?? new TicketService.Sdk.Dtos.ManagerStatsSdkDto(),
                    UserNames = response.UserNames switch
                    {
                        IDictionary<string, string> sdict => new Dictionary<string, string>(sdict),
                        IDictionary<string, object> odict => odict.Where(kv => kv.Value is string)
                                                                  .ToDictionary(kv => kv.Key, kv => (string)kv.Value),
                        _ => new Dictionary<string, string>()
                    }
                };

                return Result.Success(sdkDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching manager dashboard");
                return Result.Failure<ManagerDashboardAggregateSdkDto>(Error.Failure("GatewayError", "An error occurred while fetching manager dashboard."));
            }
        }
    }
}
