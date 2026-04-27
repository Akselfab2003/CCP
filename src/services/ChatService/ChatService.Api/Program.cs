using System.Reflection;
using CCP.ServiceDefaults;
using CCP.ServiceDefaults.Extensions;
using CCP.ServiceDefaults.Startup;
using CCP.ServiceDefaults.swagger;
using CCP.Shared.AuthContext;
using ChatService.Api.Endpoints;
using ChatService.Api.Middleware;
using ChatService.Application.ChatHub;
using ChatService.Application.ServiceCollection;
using ChatService.Application.Services.Domain;
using ChatService.Infrastructure.Persistence;
using ChatService.Infrastructure.ServiceCollection;
using Duende.AccessTokenManagement;
using Duende.IdentityModel.Client;
using IdentityService.Sdk.ServiceDefaults;
using MessagingService.Sdk.ServiceDefaults;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using TicketService.Sdk.ServiceDefaults;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.Strict;
        });

        builder.Services.AddOpenApi()
                        .AddAuthentication();

        builder.Services.AddAuthorization()
                        .AddHttpContextAccessor();



        builder.Services.AddServiceDefaults("ChatService.Api");


        if (Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
        {
            var keycloakURL = builder.Configuration.GetValue<string>("services:Keycloak:http:0") ?? throw new InvalidOperationException("KeycloakServiceUrl configuration value is required.");
            builder.Services.AddApiAuthenticationServices("ChatService.Api", "CCP", keycloak: keycloakURL);


            builder.Services.AddDbContext<ChatDbContext>(opts =>
            {
                opts.UseNpgsql(builder.Configuration.GetConnectionString("chatDB"), o => { o.UseVector(); });
            });

            builder.AddOllamaApiClient("embedding").AddKeyedEmbeddingGenerator("embedding");
            builder.AddKeyedOllamaApiClient("qwen")
                   .AddKeyedChatClient("qwen")
                   .UseFunctionInvocation()
                   .UseOpenTelemetry()
                   .UseLogging();


            // Ticket service URL.
            var ticketUrl = builder.Configuration["services:ticketservice-api:https:0"]
                            ?? builder.Configuration["services:ticketservice-api:http:0"]
                            ?? "http://localhost:5001";

            builder.Services.AddClientCredentialsTokenManagement()
                           .AddClient(ClientCredentialsClientName.Parse("CCP.ServiceAccount"), client =>
                           {
                               client.TokenEndpoint = new Uri($"{keycloakURL}/realms/CCP/protocol/openid-connect/token");
                               client.ClientId = ClientId.Parse("CCP.ServiceAccount");
                               client.ClientSecret = ClientSecret.Parse(
                                   builder.Configuration["SERVICE_ACCOUNT_SECRET"]
                                   ?? throw new InvalidOperationException("SERVICE_ACCOUNT_SECRET configuration value is required.")
                               );
                               client.Scope = Scope.ParseOrDefault("openid");
                               client.ClientCredentialStyle = ClientCredentialStyle.AuthorizationHeader;
                           });


            builder.Services.AddIdentityServiceSdk(builder.Configuration.GetValue<string>("services:identityservice-api:http:0")
                                                   ?? throw new InvalidOperationException("IdentityServiceUrl configuration value is required."), true);

            builder.Services.AddTicketServiceSdk(ticketUrl, true);

            builder.Services.AddOpenApi(op => op.SetupOpenApiForSwagger())
                .AddSwaggerGen(c => { c.SetupSwaggerForChatApp(); })
                .AddEndpointsApiExplorer();



            builder.Services.AddMessageServiceSDK(
                builder.Configuration.GetValue<string>("services:messagingservice-api:http:0")
                ?? throw new InvalidOperationException("MessagingServiceUrl configuration value is required."), true);



            builder.Services.AddSingleton<ServiceAccountOverrider>();

        }
        builder.Services.AddSignalR(options =>
        {
            options.AddFilter<HubFilter>();
        });


        builder.Services.AddControllers();
        builder.Services.AddApplicationServices();

        builder.Services.AddInfrastructureServices(builder.Configuration);



        var app = builder.Build();

        app.UseCors();

        var hubRoute = app.MapHub<ChatHub>("/chatHub");
        if (Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
        {
            hubRoute.RequireCors(c =>
            {
                c.SetIsOriginAllowed(origin =>
                {
                    using var scope = app.Services.CreateScope();
                    var domainservices = scope.ServiceProvider.GetRequiredService<IDomainServices>();
                    var host = new Uri(origin).Host;
                    return domainservices.IsDomainAllowed(host);
                })
                 .AllowAnyHeader()
                 .AllowAnyMethod()
                  .AllowCredentials();
            });
        }

        if (Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
        {
            app.AppMapSwaggerExtensions();
            app.UseMiddleware<AuthMiddleware>();
            app.UseWhen(context => context.Request.Path.StartsWithSegments("/chatHub", StringComparison.CurrentCultureIgnoreCase), conf =>
            {
                conf.UseMiddleware<UserSessionMiddleware>();

            });
            AutomaticallyApplyDBMigration<ChatDbContext>.ApplyMigrationsAsync(app).Wait();
        }

        app.MapOpenApi();
        app.MapControllers();
        app.MapSessionEndpoints()
           .MapFaqManagementEndpoints()
           .MapChatEndpoints()
           .MapConfigurationEndpoints();





        app.Run();
    }
}
