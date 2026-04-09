using EmailService.Worker.Host.Services;

namespace EmailService.Worker.Host;

public class Worker(ILogger<Worker> logger, IInboxListener inboxListener) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Email worker started");
        await inboxListener.ListenAsync(stoppingToken);
    }
}
