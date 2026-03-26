namespace MessagingService.Domain.Interfaces
{
    public interface IMessageAccessValidator
    {
        Task<bool> CanSendMessageAsync(
            int ticketId,
            Guid organizationId,
            Guid? userId,
            CancellationToken cancellationToken = default);
    }
}
