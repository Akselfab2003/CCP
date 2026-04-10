using EmailService.Domain.Interfaces;
using EmailService.Infrastructure.Data;
using EmailService.Infrastructure.EmailInfrastructure;
using EmailService.Worker.Host;
using EmailService.Worker.Host.Services;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);
var emailHostUrl = builder.Configuration.GetValue<string>("emailHostUrl") ?? throw new InvalidOperationException("emailHostUrl configuration value is required.");
builder.Services.AddSingleton<ImapMailReciver>(_ => new ImapMailReciver(emailHostUrl, builder.Configuration));
builder.Services.AddHostedService<Worker>();

builder.Services.AddDbContext<DBcontext>(option =>
{
    option.UseNpgsql(builder.Configuration.GetConnectionString("EmailDB"));
});
builder.Services.AddScoped<IEmailWorkerConfigurationRepo, TenantEmailConfigurationRepo>();

var host = builder.Build();
host.Run();
