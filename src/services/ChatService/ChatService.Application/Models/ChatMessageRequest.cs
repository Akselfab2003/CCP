namespace ChatService.Application.Models
{
    public class ChatMessageRequest
    {
        public required string Message { get; set; }
        public int TicketId { get; set; }
    }
}
