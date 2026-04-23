using System.Text.Json.Serialization;
using TicketService.Sdk.Dtos;

namespace Gateway.Sdk.Dtos
{
    public class ManagerDashboardAggregateSdkDto
    {
        [JsonPropertyName("stats")]
        public ManagerStatsSdkDto Stats { get; set; } = new();

        [JsonPropertyName("userNames")]
        public Dictionary<string, string> UserNames { get; set; } = new();
    }
}
