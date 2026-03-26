using MessagingService.Sdk.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MessagingService.Sdk.ServiceDefaults
{
    public static class ServiceRegistration
    {
        private const string ServiceName = "MessageService";

        public static IServiceCollection AddMessageServiceSDK(this IServiceCollection services, string serviceUrl, bool IsServiceAccount = false)
        {
            services.AddSdkAuthentication(ServiceName, serviceUrl, IsServiceAccount);

            services.AddScoped<IKiotaApiClient<MessagingServiceClient>>(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();

                return new KiotaApiClientAbstraction<MessagingServiceClient>(
                    httpClientFactory,
                    ServiceName,
                    requestAdapter => new MessagingServiceClient(requestAdapter));
            });

            services.AddScoped<IMessageSdkService, MessagingSdkService>();

            return services;
        }
    }
}
