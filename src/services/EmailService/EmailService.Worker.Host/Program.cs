using EmailService.Worker.Host;
using EmailService.Worker.Host.Services;

var builder = Host.CreateApplicationBuilder(args);
var emailHostUrl = builder.Configuration.GetValue<string>("emailHostUrl") ?? throw new InvalidOperationException("emailHostUrl configuration value is required.");
builder.Services.AddSingleton<ImapMailReciver>(_ => new ImapMailReciver(emailHostUrl, builder.Configuration));
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
