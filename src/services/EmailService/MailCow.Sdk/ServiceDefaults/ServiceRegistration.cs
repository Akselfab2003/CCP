using MailCow.Sdk.services.MailBox;
using Microsoft.Extensions.DependencyInjection;

namespace MailCow.Sdk.ServiceDefaults
{
    public static class ServiceRegistration
    {
        private const string MailCowClientName = "MailCowClient";

        public static IServiceCollection AddMailCowSdk(this IServiceCollection services, string serviceUrl, string apiKey)
        {
            services.AddApiKeyAuthentication(MailCowClientName, serviceUrl, apiKey);
            services.AddSingleton<IKiotaApiClient<MailCowClient>, KiotaApiClientAbstraction<MailCowClient>>(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                return new KiotaApiClientAbstraction<MailCowClient>(httpClientFactory, MailCowClientName, requestAdapter => new MailCowClient(requestAdapter));
            });

            services.AddScoped<IMailBoxManagementService, MailBoxManagementService>();

            return services;
        }
    }
}
