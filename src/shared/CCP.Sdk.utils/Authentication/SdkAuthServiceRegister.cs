using Duende.AccessTokenManagement;
using Duende.AccessTokenManagement.OpenIdConnect;
using Duende.IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;

namespace CCP.Sdk.utils.Authentication
{
    public static class SdkAuthServiceRegister
    {
        public static IServiceCollection AddSdkAuthentication(this IServiceCollection services,
                                                              string clientName,
                                                              string ServiceUrl,
                                                              bool IsServiceAccount = false)
        {

            services.AddOpenIdAccessTokenManagement(IsServiceAccount);
            services.AddHttpClientConnection(clientName, ServiceUrl, IsServiceAccount);

            return services;
        }


        private static IServiceCollection AddOpenIdAccessTokenManagement(this IServiceCollection services, bool IsDataSeeder)
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
                services.AddClientCredentialsTokenManagement();

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
                });

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
