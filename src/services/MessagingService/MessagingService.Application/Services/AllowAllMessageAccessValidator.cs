using MessagingService.Domain.Interfaces;

namespace MessagingService.Application.Services
{
    public class AllowAllMessageAccessValidator : IMessageAccessValidator
    {
        public Task<bool> CanSendMessageAsync(
            int ticketId,
            Guid organizationId,
            Guid? userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }
    }
}
