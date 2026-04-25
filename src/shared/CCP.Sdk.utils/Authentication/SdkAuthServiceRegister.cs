using CCP.Shared.AuthContext;
using Duende.AccessTokenManagement;
using Duende.AccessTokenManagement.OpenIdConnect;
using Duende.IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CCP.Sdk.utils.Authentication
{
    public static class SdkAuthServiceRegister
    {
        public static IServiceCollection AddSdkAuthentication(this IServiceCollection services,
                                                              string clientName,
                                                              string ServiceUrl,
                                                              bool IsServiceAccount = false,
                                                              IConfiguration? configuration = null)
        {

            services.AddOpenIdAccessTokenManagement(IsServiceAccount, configuration);
            services.AddHttpClientConnection(clientName, ServiceUrl, IsServiceAccount);
            services.AddTransient<TenantHeaderInjector>();
            services.AddScoped<ServiceAccountOverrider>();
            return services;
        }


        private static IServiceCollection AddOpenIdAccessTokenManagement(this IServiceCollection services, bool IsDataSeeder, IConfiguration? configuration = null)
        {
            if (!IsDataSeeder)
            {
                services
                    .AddMemoryCache()
                    .AddOpenIdConnectAccessTokenManagement(o =>
                    {
                        o.ClientCredentialStyle = ClientCredentialStyle.AuthorizationHeader;
                        o.RefreshBeforeExpiration = TimeSpan.Zero;
                    })
                    .AddBlazorServerAccessTokenManagement<ServerSideUserTokenStore>();
            }
            else
            {
                // Resolve Keycloak URL — Aspire injects this as a connection string
                var keycloakUrl = configuration?["services:keycloak:http:0"]
                               ?? configuration?["Keycloak:Authority"]
                               ?? "http://localhost:8080";

                var tokenEndpoint = keycloakUrl.TrimEnd('/') + "/realms/CCP/protocol/openid-connect/token";

                // Secret is injected by AppHost via WithEnvironment("CCP.ServiceAccount", ...)
                var serviceAccountSecret = configuration?["CCP.ServiceAccount"]
                                        ?? configuration?["SERVICE_ACCOUNT_SECRET"];

                var tokenManagement = services.AddClientCredentialsTokenManagement();

                // Only register the client if the secret is available.
                // During OpenAPI spec generation (GetDocument.Insider) the secret is not
                // present — skipping avoids a startup exception in that context.
                if (!string.IsNullOrEmpty(serviceAccountSecret))
                {
                    tokenManagement.AddClient(ClientCredentialsClientName.Parse("CCP.ServiceAccount"), client =>
                    {
                        client.TokenEndpoint = new Uri(tokenEndpoint);
                        client.ClientId = ClientId.Parse("CCP.ServiceAccount");
                        client.ClientSecret = ClientSecret.Parse(serviceAccountSecret);
                        client.Scope = Scope.ParseOrDefault("openid");
                        client.ClientCredentialStyle = ClientCredentialStyle.AuthorizationHeader;
                    });
                }
            }

            return services;
        }



        private static IServiceCollection AddHttpClientConnection(this IServiceCollection services,
                                                                  string ClientName,
                                                                  string ServiceUrl,
                                                                  bool IsServiceAccount)
        {
            if (IsServiceAccount)
            {
                services.AddClientCredentialsHttpClient(ClientName, ClientCredentialsClientName.Parse("CCP.ServiceAccount"), client =>
                {
                    client.BaseAddress = new Uri(ServiceUrl);
                }).AddHttpMessageHandler<TenantHeaderInjector>();

                return services;
            }

            services.AddUserAccessTokenHttpClient(ClientName, configureClient: client =>
            {
                client.BaseAddress = new Uri(ServiceUrl);
            });

            return services;
        }

        public static IServiceCollection AddKeycloakHttpConnection(this IServiceCollection services, string ClientName, string ServiceUrl)
        {
            services.AddClientCredentialsHttpClient(ClientName, ClientCredentialsClientName.Parse("KeyCloak.Admin"), client =>
            {
                client.BaseAddress = new Uri(ServiceUrl);
            });
            return services;
        }


        public static IServiceCollection AddApiKeyAuthentication(this IServiceCollection services, string ClientName, string ServiceUrl, string ApiKey)
        {
            services.AddHttpClient(ClientName, client =>
            {
                client.BaseAddress = new Uri(ServiceUrl);
                client.DefaultRequestHeaders.Add("X-API-Key", ApiKey);
            });
            return services;

        }
    }
}
