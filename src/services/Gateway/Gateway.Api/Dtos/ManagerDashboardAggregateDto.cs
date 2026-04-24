using TicketService.Sdk.Dtos;

namespace Gateway.Api.Dtos
{
    public class ManagerDashboardAggregateDto
    {
        public ManagerStatsSdkDto Stats { get; set; } = new();
        public Dictionary<string, string> UserNames { get; set; } = new();
    }
}
