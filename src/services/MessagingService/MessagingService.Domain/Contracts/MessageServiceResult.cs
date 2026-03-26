namespace MessagingService.Domain.Contracts
{
    public class MessageServiceResult
    {
        public bool Success { get; set; }

        public string? ErrorMessage { get; set; }

        public MessageResponse? Message { get; set; }

        public static MessageServiceResult Failed(string errorMessage) =>
            new()
            {
                Success = false,
                ErrorMessage = errorMessage
            };

        public static MessageServiceResult Succeeded(MessageResponse message) =>
            new()
            {
                Success = true,
                Message = message
            };
    }
}
