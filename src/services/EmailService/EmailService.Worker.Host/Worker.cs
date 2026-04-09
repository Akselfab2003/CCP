using EmailService.Worker.Host.Services;

namespace EmailService.Worker.Host;

public class Worker(ILogger<Worker> logger, ImapMailReciver imapMailReciver) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            imapMailReciver.ConnectAsync().GetAwaiter().GetResult();
            imapMailReciver.ListenerAsync().GetAwaiter().GetResult();
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}
