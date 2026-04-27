using System.Reflection;
using Duende.AccessTokenManagement;
using Duende.IdentityModel.Client;
using EmailService.Sdk.ServiceDefaults;
using IdentityService.Sdk.ServiceDefaults;
using MessagingService.Api.Hubs;
using MessagingService.Application.ServiceCollection;
using MessagingService.Infrastructure.Persistence;
using MessagingService.Infrastructure.ServiceCollection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using TicketService.Sdk.ServiceDefaults;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.ConfigureDefaultOpenTelemetry("MessagingService.Api");
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options =>
{
    OpenApiConfiguration.SetupOpenApiForSwagger(options);
});


if (Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
{

    var keycloakServiceUrl =
    builder.Configuration.GetValue<string>("services:Keycloak:http:0")
    ?? throw new InvalidOperationException("KeycloakServiceUrl configuration value is required.");

    builder.Services.AddDbContext<MessagingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MessagingDatabase"),
                      o => o.UseVector()));

    builder.Services.AddSwaggerGen(c => { SetupSwagger.SetupSwaggerForChatApp(c); });
    builder.Services.AddApiAuthenticationServices("MessagingService.Api", "CCP");

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

    builder.UseWolverine(opts =>
    {
        opts.UseRabbitMq(builder.Configuration.GetConnectionString("RabbitMQ")!)
            .AutoProvision();

        opts.ListenToRabbitQueue("ticket.assignment.updated")
            .CircuitBreaker(c =>
            {
                c.FailurePercentageThreshold = 10;
                c.PauseTime = TimeSpan.FromMinutes(1);
            })
            .UseDurableInbox();
    });
    builder.Services.AddTicketServiceSdk(
        builder.Configuration.GetConnectionString("ticketservice-api") ?? builder.Configuration["services:ticketservice-api:https:0"] ?? string.Empty,
        IsServiceAccount: true,
        configuration: builder.Configuration);
    builder.Services.AddEmailServiceSdk(
    builder.Configuration.GetValue<string>("services:emailservice-api:http:0")
    ?? throw new InvalidOperationException("EmailServiceUrl configuration value is required."),true);
    builder.Services.AddIdentityServiceSdk(
    builder.Configuration.GetValue<string>("services:identityservice-api:http:0")
    ?? throw new InvalidOperationException("IdentityServiceUrl configuration value is required."));

    builder.Services.AddSingleton<ServiceAccountOverrider>();
}

builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddHttpContextAccessor();
var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
if (Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
{
    AutomaticallyApplyDBMigration<MessagingDbContext>.ApplyMigrationsAsync(app).Wait();
    app.AppMapSwaggerExtensions();
    app.UseMiddleware<AuthMiddleware>();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapHub<ChatHub>("/chathub");

app.Run();

namespace TestProgramNameSpace
{
    public partial class Program
    {
    }
}
