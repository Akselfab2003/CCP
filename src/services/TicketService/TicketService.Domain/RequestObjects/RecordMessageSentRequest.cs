namespace TicketService.Domain.RequestObjects
{
    public class RecordMessageSentRequest
    {
        public Guid? SenderUserId { get; set; }
        public string MessageSnippet { get; set; } = string.Empty;
    }
}
