using CCP.ServiceDefaults;
using CCP.Shared.AuthContext;
using CustomerService.Sdk.ServiceDefaults;
using Duende.AccessTokenManagement;
using Duende.IdentityModel.Client;
using EmailService.Domain.Interfaces;
using EmailService.Infrastructure.Data;
using EmailService.Infrastructure.EmailInfrastructure;
using EmailService.Infrastructure.ServiceDefaults;
using EmailService.Worker.Host.Services;
using MessagingService.Sdk.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using TicketService.Sdk.ServiceDefaults;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddServiceDefaults("EmailWorker.Host");
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

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

var keycloakServiceUrl =
    builder.Configuration.GetValue<string>("services:Keycloak:http:0")
    ?? throw new InvalidOperationException("KeycloakServiceUrl configuration value is required.");

builder.Services.AddClientCredentialsTokenManagement()
                        .AddClient(ClientCredentialsClientName.Parse("CCP.ServiceAccount"), client =>
                        {
                            client.TokenEndpoint = new Uri($"{keycloakServiceUrl}/realms/CCP/protocol/openid-connect/token");
                            client.ClientId = ClientId.Parse("CCP.ServiceAccount");
                            client.ClientSecret = ClientSecret.Parse(
                                builder.Configuration["SERVICE_ACCOUNT_SECRET"]
                                ?? throw new InvalidOperationException("SERVICE_ACCOUNT_SECRET configuration value is required.")
                            );
                            client.Scope = Scope.ParseOrDefault("openid");
                            client.ClientCredentialStyle = ClientCredentialStyle.AuthorizationHeader;
                        });

builder.Services.AddCustomerviceSdk(
    builder.Configuration.GetValue<string>("services:customerservice-api:http:0")
    ?? throw new InvalidOperationException("CustomerServiceUrl configuration value is required."), true);

builder.Services.AddTicketServiceSdk(
    builder.Configuration.GetValue<string>("services:ticketservice-api:http:0")
    ?? throw new InvalidOperationException("TicketServiceUrl configuration value is required."), true);

builder.Services.AddMessageServiceSDK(
    builder.Configuration.GetValue<string>("services:messagingservice-api:http:0")
    ?? throw new InvalidOperationException("MessageServiceUrl configuration value is required."), true);



builder.Services.AddSingleton<ServiceAccountOverrider>();

builder.Services.AddDbContext<DBcontext>(option =>
{
    option.UseNpgsql(builder.Configuration.GetConnectionString("EmailDB"));
});

var mailServer = builder.Configuration.GetValue<string>("emailHostUrl") ?? throw new InvalidOperationException("emailHostUrl configuration value is required.");
builder.Services.AddScoped<IMailBoxService, MailBoxService>(s => new MailBoxService(logger: s.GetRequiredService<ILogger<MailBoxService>>(),
                                                                                    emailWorkerConfigurationRepo: s.GetRequiredService<IEmailWorkerConfigurationRepo>(),
                                                                                    emailhostUrl: mailServer));
builder.Services.AddScoped<IEmailWorkerConfigurationRepo, TenantEmailConfigurationRepo>();
builder.Services.AddScoped<IMailProcessingService, MailProcessingService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddInfrastructureServices(builder.Configuration);
var host = builder.Build();
host.Run();
