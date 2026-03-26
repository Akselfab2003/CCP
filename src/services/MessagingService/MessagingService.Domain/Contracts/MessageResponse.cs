namespace MessagingService.Domain.Contracts
{
    public class MessageResponse
    {
        public int Id { get; set; }

        public int TicketId { get; set; }

        public Guid? UserId { get; set; }

        public Guid OrganizationId { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? UpdatedAtUtc { get; set; }

        public bool IsEdited { get; set; }

        public bool IsDeleted { get; set; }
        public bool IsInternalNote { get; set; }

        public DateTime? DeletedAtUtc { get; set; }

        public float[]? Embedding { get; set; }
    }
}
