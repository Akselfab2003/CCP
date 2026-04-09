using EmailService.Worker.Host;
using EmailService.Worker.Host.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<IInboxListener, ImapMailReciver>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
