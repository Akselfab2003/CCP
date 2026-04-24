using System.Text.Json.Serialization;

namespace TicketService.Sdk.Dtos
{
    public class ManagerStatsSdkDto
    {
        [JsonPropertyName("openTickets")]
        public int OpenTickets { get; set; }

        [JsonPropertyName("closedToday")]
        public int ClosedToday { get; set; }

        [JsonPropertyName("avgResponseTime")]
        public string AvgResponseTime { get; set; } = "—";

        [JsonPropertyName("awaitingUser")]
        public int AwaitingUser { get; set; }

        [JsonPropertyName("teamPerformance")]
        public List<SupporterPerformanceSdkDto> TeamPerformance { get; set; } = new();
    }
}
