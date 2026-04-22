using System.Reflection;
using Gateway.Api.Endpoints;
using IdentityService.Sdk.ServiceDefaults;
using MessagingService.Sdk.ServiceDefaults;
using TicketService.Sdk.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.ConfigureDefaultOpenTelemetry("Gateway.Api");
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options =>
{
    OpenApiConfiguration.SetupOpenApiForSwagger(options);
});

if (Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
{
    var keycloakURL = builder.Configuration.GetValue<string>("services:Keycloak:http:0")
        ?? throw new InvalidOperationException("KeycloakServiceUrl configuration value is required.");
    builder.Services.AddApiAuthenticationServices("Gateway.Api", "CCP", keycloakURL);

    builder.Services.AddSwaggerGen(c => { SetupSwagger.SetupSwaggerForChatApp(c); });
}

builder.Services.AddTicketServiceSdk(
    builder.Configuration.GetConnectionString("ticketservice-api")
        ?? builder.Configuration["services:ticketservice-api:https:0"]
        ?? string.Empty,
    IsServiceAccount: true,
    configuration: builder.Configuration);

builder.Services.AddMessageServiceSDK(
    builder.Configuration.GetConnectionString("messagingservice-api")
        ?? builder.Configuration["services:messagingservice-api:http:0"]
        ?? string.Empty,
    IsServiceAccount: true,
    configuration: builder.Configuration);

builder.Services.AddIdentityServiceSdk(
    builder.Configuration.GetConnectionString("identityservice-api")
        ?? builder.Configuration["services:identityservice-api:http:0"]
        ?? string.Empty,
    IsServiceAccount: true,
    configuration: builder.Configuration);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
{
    app.AppMapSwaggerExtensions();
    app.UseMiddleware<AuthMiddleware>();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapGatewayEndpoints();

app.Run();

namespace TestProgramNameSpace
{
    public partial class Program { }
}
