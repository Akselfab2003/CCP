using System.Reflection;
using CCP.Shared.Events;
using Duende.AccessTokenManagement;
using Duende.IdentityModel.Client;
using EmailService.Sdk.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using TicketService.Api.Endpoints;
using TicketService.Application.ServiceDefaults;
using TicketService.Infrastructure.Persistence;
using TicketService.Infrastructure.ServiceCollection;
using Wolverine;
using Wolverine.RabbitMQ;
using IdentityService.Sdk.ServiceDefaults;

namespace TicketService.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.Strict;
            });

            builder.Services.AddOpenApi(op => OpenApiConfiguration.SetupOpenApiForSwagger(op));

            builder.Services.AddAuthentication();
            builder.Services.AddAuthorization();
            builder.Services.AddHttpContextAccessor();
            builder.Services.ConfigureDefaultOpenTelemetry("TicketService.Api");

            if (Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
            {
                var keycloakURL = builder.Configuration.GetValue<string>("services:Keycloak:http:0")
                    ?? throw new InvalidOperationException("KeycloakServiceUrl configuration value is required.");
                builder.Services.AddApiAuthenticationServices("TicketService.Api", "CCP", keycloakURL);
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


                builder.Services.AddDbContext<TicketDbContext>(options =>
                {
                    options.UseNpgsql(builder.Configuration.GetConnectionString("TicketDb"));
                });

                builder.Services.AddSwaggerGen(c => { SetupSwagger.SetupSwaggerForChatApp(c); });

                builder.UseWolverine(opts =>
                {
                    opts.UseRabbitMq(builder.Configuration.GetConnectionString("RabbitMQ")!)
                        .AutoProvision();

                    opts.PublishMessage<TicketAssignmentUpdated>()
                        .ToRabbitQueue("ticket.assignment.updated")
                        .UseDurableOutbox();

                    opts.PublishMessage<TicketCreated>()
                        .ToRabbitQueue("ticket.created")
                        .UseDurableOutbox();

                    opts.PublishMessage<TicketClosed>()
                        .ToRabbitQueue("ticket.closed")
                        .UseDurableOutbox();

                });




                builder.Services.AddEmailServiceSdk(
                    builder.Configuration.GetValue<string>("services:emailservice-api:http:0")
                    ?? throw new InvalidOperationException("EmailServiceUrl configuration value is required."), true);

                builder.Services.AddIdentityServiceSdk(
                    builder.Configuration.GetValue<string>("services:identityservice-api:http:0")
                    ?? throw new InvalidOperationException("IdentityServiceUrl configuration value is required."));

            }

            builder.Services.AddApplication();
            builder.Services.AddInfrastructure();
            builder.Services.AddSingleton<ServiceAccountOverrider>();

            var app = builder.Build();

            app.UseAuthentication();
            app.UseAuthorization();

            if (Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
            {
                app.AppMapSwaggerExtensions();
                app.UseMiddleware<AuthMiddleware>();
                AutomaticallyApplyDBMigration<TicketDbContext>.ApplyMigrationsAsync(app).Wait();
            }

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            app.MapTicketEndpoint()
               .MapAssignmentEndpoints();

            app.Run();
        }
    }
}
