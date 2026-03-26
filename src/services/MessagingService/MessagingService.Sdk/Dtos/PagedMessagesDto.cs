namespace MessagingService.Sdk.Dtos
{
    public class PagedMessagesDto
    {
        public IReadOnlyList<MessageDto> Items { get; set; } = Array.Empty<MessageDto>();

        public bool HasMore { get; set; }
    }
}
