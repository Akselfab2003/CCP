using MessagingService.Domain.Contracts;

namespace MessagingService.Domain.Interfaces
{
    public interface IMessageService
    {
        Task<MessageServiceResult> CreateMessageAsync(CreateMessageRequest request, CancellationToken cancellationToken = default);

        Task<PagedMessagesResponse> GetMessagesByTicketIdAsync(
            int ticketId,
            int limit,
            int? beforeMessageId,
            CancellationToken cancellationToken = default);

        Task<MessageResponse?> GetMessageByIdAsync(int messageId, CancellationToken cancellationToken = default);

        Task<MessageServiceResult> UpdateMessageAsync(int messageId, UpdateMessageRequest request, CancellationToken cancellationToken = default);

        Task<(bool Deleted, int? TicketId)> SoftDeleteMessageAsync(int messageId, CancellationToken cancellationToken = default);
    }
}
