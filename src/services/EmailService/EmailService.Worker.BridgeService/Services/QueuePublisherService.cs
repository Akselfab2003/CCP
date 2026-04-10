using System.Text.Json;
using CCP.Shared.Events;
using CCP.Shared.ResultAbstraction;
using JasperFx.Core;
using Wolverine;

namespace EmailService.Worker.BridgeService.Services
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

        public async Task<Result> PublishEmailMessageAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            try
            {
                if (stream == null)
                {
                    _logger.LogWarning("Attempted to publish an empty email message");
                    return Result.Failure(Error.Failure(code: "EmptyEmailMessage", description: "Email message cannot be empty"));
                }

                var json = await stream.ReadAllTextAsync();
                mail_received? emailMessage = JsonSerializer.Deserialize<mail_received>(json);

                if (emailMessage == null)
                {
                    _logger.LogWarning("Failed to deserialize email message");
                    return Result.Failure(Error.Failure(code: "InvalidEmailMessage", description: "Email message is invalid"));
                }

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
