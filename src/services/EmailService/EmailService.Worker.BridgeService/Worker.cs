using System.Net.Sockets;
using EmailService.Worker.BridgeService.Services;

namespace EmailService.Worker.BridgeService
{
    public class Worker(ILogger<Worker> logger, IServiceProvider serviceProvider) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var queuePublisher = scope.ServiceProvider.GetRequiredService<IQueuePublisherService>();

                using (var listener = new TcpListener(System.Net.IPAddress.Any, 5000))
                {
                    listener.Start();

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var client = await listener.AcceptTcpClientAsync(stoppingToken);
                        var publishResult = await queuePublisher.PublishEmailMessageAsync(client.GetStream(), stoppingToken);
                        if (logger.IsEnabled(LogLevel.Information))
                        {
                            logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                        }
                    }
                }
            }
        }
    }
}
