using System.Text.Json.Serialization;

namespace TicketService.Sdk.Dtos
{
    public class SupporterPerformanceSdkDto
    {
        [JsonPropertyName("userId")]
        public Guid UserId { get; set; }

        [JsonPropertyName("resolvedCount")]
        public int ResolvedCount { get; set; }
    }
}
