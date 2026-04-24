using CCP.Shared.Events;
using CCP.Shared.ResultAbstraction;

namespace EmailService.Application.Interfaces
{
    public interface IQueuePublisherService
    {
        Task<Result> PublishEmailMessageAsync(DovecotEvent emailEvent, CancellationToken cancellationToken = default);
    }
}
