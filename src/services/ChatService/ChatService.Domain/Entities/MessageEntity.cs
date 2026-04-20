namespace ChatService.Domain.Entities
{
    public class MessageEntity
    {
        public Guid Id { get; set; }
        public Guid OrgId { get; set; }
        public Guid ConversationId { get; set; }
        public required string Message { get; set; }
        public bool IsFromUser { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
