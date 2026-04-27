
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



                //var sdkDto = new Dtos.TicketDetailAggregateSdkDto
                //{
                //    Ticket = Ticket,
                //    Messages = messages,
                //    UserNames = dto.UserNames
                //};


                return dto is not null
                    ? Result.Success(new TicketDetailAggregateSdkDto())
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
                    return Result.Failure<Dtos.ManagerDashboardAggregateSdkDto>(Error.Failure("GatewayError", "Gateway returned an empty response."));
                }
                managerStats = response.Stats != null
                                    ? new ManagerDashboardAggregateSdkDto
                                    {

                                        Stats = new TicketService.Sdk.Dtos.ManagerStatsSdkDto()
                                        {
                                            AvgResponseTime = response.Stats.AvgResponseTime ?? string.Empty,
                                            AwaitingUser = response.Stats.AwaitingUser ?? 0,
                                            ClosedToday = response.Stats.ClosedToday ?? 0,
                                            OpenTickets = response.Stats.OpenTickets ?? 0,
                                            TeamPerformance = new List<TicketService.Sdk.Dtos.SupporterPerformanceSdkDto>()
                                            {
                                            }
                                        }
                                    }
                                    : null;



                return managerStats is not null
                    ? Result.Success(new ManagerDashboardAggregateSdkDto { })
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
