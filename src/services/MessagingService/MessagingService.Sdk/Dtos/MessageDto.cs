using System.Text.Json.Serialization;

namespace MessagingService.Sdk.Dtos
{
    public class MessageDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("ticketId")]
        public int TicketId { get; set; }

        [JsonPropertyName("userId")]
        public Guid? UserId { get; set; }

        [JsonPropertyName("organizationId")]
        public Guid OrganizationId { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("createdAtUtc")]
        public DateTimeOffset? CreatedAtUtc { get; set; }

        [JsonPropertyName("updatedAtUtc")]
        public DateTimeOffset? UpdatedAtUtc { get; set; }

        [JsonPropertyName("isEdited")]
        public bool IsEdited { get; set; }

        [JsonPropertyName("isDeleted")]
        public bool IsDeleted { get; set; }

        [JsonPropertyName("isInternalNote")]
        public bool IsInternalNote { get; set; }

        [JsonPropertyName("deletedAtUtc")]
        public DateTimeOffset? DeletedAtUtc { get; set; }

        [JsonPropertyName("attachmentUrl")]
        public string? AttachmentUrl { get; set; }

        [JsonPropertyName("attachmentFileName")]
        public string? AttachmentFileName { get; set; }

        [JsonPropertyName("attachmentContentType")]
        public string? AttachmentContentType { get; set; }
    }
}
