using CCP.Sdk.utils.Abstractions;
using CCP.Shared.ResultAbstraction;
using Microsoft.Extensions.Logging;

namespace ChatService.Sdk.Services
{
    internal class ChatClient : IChatService
    {
        private readonly ILogger<ChatClient> _logger;
        private readonly IKiotaApiClient<ChatServiceClient> _apiClient;

        public ChatClient(ILogger<ChatClient> logger, IKiotaApiClient<ChatServiceClient> apiClient)
        {
            _logger = logger;
            _apiClient = apiClient;
        }

        public async Task<Result> SendMessageToChatbotTicket(int TicketId, string message)
        {
            try
            {
                await _apiClient.Client.Chat.Message.PostAsync(new Models.ChatMessageRequest()
                {
                    Message = message,
                    TicketId = TicketId
                });


                return Result.Success();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message to chatbot for ticket {TicketId}", TicketId);
                return Result.Failure(Error.Failure(code: "SendMessageError", description: $"An error occurred while sending the message: {ex.Message}"));
            }
        }
    }
}
