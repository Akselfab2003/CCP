using CCP.ServiceDefaults.Extensions;
using EmailService.Worker.BridgeService.Services;
using Wolverine;
using Wolverine.RabbitMQ;

namespace EmailService.Worker.BridgeService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddHostedService<Worker>();
            builder.Services.ConfigureDefaultOpenTelemetry("EmailService.Worker.BridgeService");
            builder.Services.AddScoped<IQueuePublisherService, QueuePublisherService>();

            builder.UseWolverine(opts =>
            {
                opts.UseRabbitMq(builder.Configuration.GetConnectionString("RabbitMQ")!)
                    .AutoProvision();

                opts.PublishAllMessages().ToRabbitQueue("mailbox.queue").UseDurableOutbox();
            });

            var host = builder.Build();
            host.Run();
        }
    }
}
