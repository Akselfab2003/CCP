using CCP.Shared.Events;
using CCP.Shared.ResultAbstraction;
using EmailService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace EmailService.Application.Services
{
    public class QueuePublisherService : IQueuePublisherService
    {
        private readonly ILogger<QueuePublisherService> _logger;
        private readonly IMessageBus _messageBus;

        public QueuePublisherService(ILogger<QueuePublisherService> logger, IMessageBus messageBus)
        {
            _logger = logger;
            _messageBus = messageBus;
        }

        public async Task<Result> PublishEmailMessageAsync(DovecotEvent emailEvent, CancellationToken cancellationToken = default)
        {
            try
            {
                if (emailEvent == null)
                {
                    _logger.LogWarning("Attempted to publish an empty email message");
                    return Result.Failure(Error.Failure(code: "EmptyEmailMessage", description: "Email message cannot be empty"));
                }

                var emailMessage = new mail_received
                {
                    @event = emailEvent.Event,
                    hostname = emailEvent.Hostname,
                    user = emailEvent.GetString("user"),
                };

                await _messageBus.PublishAsync<mail_received>(emailMessage);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while publishing email message");
                return Result.Failure(Error.Failure(code: "PublishEmailMessageException", description: $"An exception occurred while publishing email message: {ex.Message}"));
            }
        }
    }
}
