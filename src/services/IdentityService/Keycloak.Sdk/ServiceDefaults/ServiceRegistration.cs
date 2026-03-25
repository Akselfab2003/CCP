using CCP.Sdk.utils.Abstractions;
using CCP.Sdk.utils.Authentication;
using Keycloak.Sdk.services.groups;
using Keycloak.Sdk.services.management;
using Keycloak.Sdk.services.members;
using Keycloak.Sdk.services.organizations;
using Keycloak.Sdk.services.users;
using Microsoft.Extensions.DependencyInjection;

namespace Keycloak.Sdk.ServiceDefaults
{
    public static class ServiceRegistration
    {
        private const string KeycloakClientName = "KeycloakClient";

        public static IServiceCollection AddKeycloakSdk(this IServiceCollection services, string serviceUrl)
        {
            services.AddKeycloakHttpConnection(KeycloakClientName, serviceUrl);
            services.AddSingleton<IKiotaApiClient<KeycloakClient>, KiotaApiClientAbstraction<KeycloakClient>>(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                return new KiotaApiClientAbstraction<KeycloakClient>(httpClientFactory, KeycloakClientName, requestAdapter => new KeycloakClient(requestAdapter));
            });

            services.AddScoped<IUserKeycloakService, UserServices>()
                    .AddScoped<IGroupKeycloakService, GroupServices>()
                    .AddScoped<IOrganizationKeycloakService, OrganizationService>()
                    .AddScoped<IMemberKeycloakService, MemberService>()
                    .AddScoped<IManagementKeycloakService, ManagementService>();

            return services;
        }
    }
}
