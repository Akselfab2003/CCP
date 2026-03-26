namespace MessagingService.Domain.Contracts
{
    public class UpdateMessageRequest
    {
        public string Content { get; set; } = string.Empty;

        public float[]? Embedding { get; set; }
    }
}
