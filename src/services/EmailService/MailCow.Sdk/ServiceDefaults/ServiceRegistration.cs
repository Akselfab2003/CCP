using CCP.Sdk.utils.Abstractions;
using CCP.Sdk.utils.Authentication;
using MailCow.Sdk;
using Microsoft.Extensions.DependencyInjection;

namespace Keycloak.Sdk.ServiceDefaults
{
    public static class ServiceRegistration
    {
        private const string MailCowClientName = "MailCowClient";

        public static IServiceCollection AddKeycloakSdk(this IServiceCollection services, string serviceUrl, string apiKey)
        {
            services.AddApiKeyAuthentication(MailCowClientName, serviceUrl, apiKey);
            services.AddSingleton<IKiotaApiClient<MailCowClient>, KiotaApiClientAbstraction<MailCowClient>>(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                return new KiotaApiClientAbstraction<MailCowClient>(httpClientFactory, MailCowClientName, requestAdapter => new MailCowClient(requestAdapter));
            });

            return services;
        }
    }
}
