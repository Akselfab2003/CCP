namespace ChatService.Domain.Dtos
{
    public class TicketMessage
    {
        public int MessageId { get; set; }
        public int TicketId { get; set; }
        public MessageAuthorType AuthorType { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }

    public enum MessageAuthorType
    {
        User,
        Support,
    }
}
