namespace MessagingService.Domain.Contracts
{
    public class PagedMessagesResponse
    {
        public IReadOnlyList<MessageResponse> Items { get; set; } = Array.Empty<MessageResponse>();

        public bool HasMore { get; set; }
    }
}
