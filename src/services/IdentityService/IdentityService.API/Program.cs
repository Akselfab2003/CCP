using System.Reflection;
using CCP.ServiceDefaults.Extensions;
using CCP.ServiceDefaults.swagger;
using CCP.Shared.AuthContext;
using Duende.AccessTokenManagement;
using Duende.IdentityModel.Client;
using IdentityService.API.Endpoints;
using IdentityService.Application.DependencyInjection;
using Keycloak.Sdk.ServiceDefaults;

namespace IdentityService.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            // Add services to the container.
            builder.Services.AddAuthentication();
            builder.Services.AddAuthorization();
            builder.Services.ConfigureDefaultOpenTelemetry("IdentityService.API");
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddApiAuthenticationServices("IdentityService.API", "CCP");


            if (Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
            {
                var keycloakServiceUrl = builder.Configuration.GetValue<string>("services:Keycloak:http:0") ?? throw new InvalidOperationException("KeycloakServiceUrl configuration value is required.");
                builder.Services.AddKeycloakSdk(keycloakServiceUrl);


                builder.Services.AddOpenApi(op => OpenApiConfiguration.SetupOpenApiForSwagger(op));
                builder.Services.AddSwaggerGen(c => { SetupSwagger.SetupSwaggerForChatApp(c); });
                builder.Services.AddApplication();

                builder.Services.AddClientCredentialsTokenManagement()
                                .AddClient(ClientCredentialsClientName.Parse("KeyCloak.Admin"), client =>
                                {
                                    client.TokenEndpoint = new Uri($"{keycloakServiceUrl}/realms/CCP/protocol/openid-connect/token");
                                    client.ClientId = ClientId.Parse("KeycloakAdminApiClient");
                                    client.ClientSecret = ClientSecret.Parse(builder.Configuration["KeycloakAdminApiClientSecret"] ?? throw new InvalidOperationException("KeycloakAdminApiClientSecret configuration value is required."));
                                    client.Scope = Scope.ParseOrDefault("openid");
                                    client.ClientCredentialStyle = ClientCredentialStyle.AuthorizationHeader;
                                });
            }

            var app = builder.Build();

            app.UseAuthentication();
            app.UseAuthorization();

            if (Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
            {
                app.AppMapSwaggerExtensions();
                app.UseMiddleware<AuthMiddleware>();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.MapUserEndpoints()
               .MapTenantEndpoints()
               .MapCustomerEndpoints();

            app.Run();
        }
    }
}
