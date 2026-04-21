namespace ChatService.Application.Models
{
    public class ChatMessageRequest
    {
        public required string Message { get; set; }
        public Guid? ConversationId { get; set; }
    }
}
