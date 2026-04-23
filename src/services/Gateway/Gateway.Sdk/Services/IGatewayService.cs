using Gateway.Sdk.Dtos;

namespace Gateway.Sdk.Services
{
    public interface IGatewayService
    {
        Task<Result<TicketDetailAggregateSdkDto>> GetTicketDetailAsync(int ticketId, CancellationToken ct = default);
        Task<Result<ManagerDashboardAggregateSdkDto>> GetManagerDashboardAsync(CancellationToken ct = default);
    }
}
