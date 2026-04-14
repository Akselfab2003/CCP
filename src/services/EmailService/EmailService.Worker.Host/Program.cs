using EmailService.Domain.Interfaces;
using EmailService.Infrastructure.Data;
using EmailService.Infrastructure.EmailInfrastructure;
using EmailService.Worker.Host;
using Microsoft.EntityFrameworkCore;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);
//var emailHostUrl = builder.Configuration.GetValue<string>("emailHostUrl") ?? throw new InvalidOperationException("emailHostUrl configuration value is required.");
//builder.Services.AddSingleton<ImapMailReciver>(_ => new ImapMailReciver(emailHostUrl, builder.Configuration));
builder.Services.AddHostedService<Worker>();

builder.UseWolverine(opts =>
{
    opts.UseRabbitMq(builder.Configuration.GetConnectionString("RabbitMQ")!)
        .AutoProvision();

    opts.ListenToRabbitQueue("mailbox.queue")
        .CircuitBreaker(c =>
        {
            c.FailurePercentageThreshold = 10;
            c.PauseTime = TimeSpan.FromMinutes(1);
        })
        .UseDurableInbox();

});

builder.Services.AddDbContext<DBcontext>(option =>
{
    option.UseNpgsql(builder.Configuration.GetConnectionString("EmailDB"));
});
builder.Services.AddScoped<IEmailWorkerConfigurationRepo, TenantEmailConfigurationRepo>();

var host = builder.Build();
host.Run();
