namespace MessagingService.Sdk.Dtos
{
    public class MessageDto
    {
        public int Id { get; set; }

        public int TicketId { get; set; }

        public Guid? UserId { get; set; }

        public Guid OrganizationId { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTimeOffset? CreatedAtUtc { get; set; }

        public DateTimeOffset? UpdatedAtUtc { get; set; }

        public bool IsEdited { get; set; }

        public bool IsDeleted { get; set; }
        public bool IsInternalNote { get; set; }

        public DateTimeOffset? DeletedAtUtc { get; set; }
    }
}
