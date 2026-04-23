using System.Text.Json.Serialization;

namespace TicketService.Sdk.Dtos
{
    public class TicketSdkDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("organizationId")]
        public Guid OrganizationId { get; set; }

        [JsonPropertyName("customerId")]
        public Guid? CustomerId { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTimeOffset? CreatedAt { get; set; }

        // Assignment info (flattened for convenience)
        [JsonPropertyName("assignedUserId")]
        public Guid? AssignedUserId { get; set; }

        [JsonPropertyName("assignedByUserId")]
        public Guid? AssignedByUserId { get; set; }
    }
}
