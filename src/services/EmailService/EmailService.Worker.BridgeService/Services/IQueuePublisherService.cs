using CCP.Shared.ResultAbstraction;

namespace EmailService.Worker.BridgeService.Services
{
    public interface IQueuePublisherService
    {
        Task<Result> PublishEmailMessageAsync(Stream stream, CancellationToken cancellationToken = default);
    }
}
