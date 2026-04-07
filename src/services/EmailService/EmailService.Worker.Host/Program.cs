using EmailService.Worker.Host;
using EmailService.Worker.Host.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<ImapMailReciver>(_ => new ImapMailReciver("localhost", builder.Configuration));
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
