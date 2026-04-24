using MessagingService.Sdk.Dtos;

namespace MessagingService.Sdk.Services
{
    public interface IMessageSdkService
    {
        Task<Result<MessageDto>> CreateMessageAsync(
            int ticketId,
            Guid organizationId,
            Guid? userId,
            string content,
            bool isInternalNote = false,
            string? attachmentUrl = null,
            string? attachmentFileName = null,
            string? attachmentContentType = null,
            CancellationToken cancellationToken = default);

        Task<Result<AttachmentDto>> UploadAttachmentAsync(
            Stream fileStream,
            string fileName,
            string contentType,
            CancellationToken cancellationToken = default);

        Task<Result<PagedMessagesDto>> GetMessagesByTicketIdAsync(
            int ticketId,
            int limit = 50,
            int? beforeMessageId = null,
            CancellationToken cancellationToken = default);

        Task<Result<MessageDto>> GetMessageByIdAsync(
            int messageId,
            CancellationToken cancellationToken = default);

        Task<Result<MessageDto>> UpdateMessageAsync(
            int messageId,
            string content,
            CancellationToken cancellationToken = default);

        Task<Result> DeleteMessageAsync(
            int messageId,
            CancellationToken cancellationToken = default);
    }
}
