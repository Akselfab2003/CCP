using System.Text.Json.Serialization;

namespace TicketService.Sdk.Dtos
{
    public class TicketHistoryEntryDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("ticketId")]
        public int TicketId { get; set; }

        [JsonPropertyName("actorUserId")]
        public Guid? ActorUserId { get; set; }

        [JsonPropertyName("eventType")]
        public string EventType { get; set; } = string.Empty;

        [JsonPropertyName("oldValue")]
        public string? OldValue { get; set; }

        [JsonPropertyName("newValue")]
        public string? NewValue { get; set; }

        [JsonPropertyName("occurredAt")]
        public DateTimeOffset OccurredAt { get; set; }
    }
}
