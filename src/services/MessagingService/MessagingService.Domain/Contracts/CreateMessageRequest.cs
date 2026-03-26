namespace MessagingService.Domain.Contracts
{
    public class CreateMessageRequest
    {
        public int TicketId { get; set; }

        public Guid? UserId { get; set; }

        public Guid OrganizationId { get; set; }

        public string Content { get; set; } = string.Empty;

        public float[]? Embedding { get; set; }
        public bool IsInternalNote { get; set; }
    }
}
