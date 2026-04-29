namespace ChatService.Domain.Entities
{
    public class ConversationEntity
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public Guid OrgId { get; set; }
        public List<MessageEntity> Messages { get; set; } = [];
        public DateTime CreatedAt { get; set; }
        public bool IsEscalated { get; set; } = false;
        public int? EscalatedTicketId { get; set; }
    }
}
