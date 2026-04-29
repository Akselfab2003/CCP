using Gateway.Sdk.Services;
using Microsoft.Extensions.Configuration;

namespace Gateway.Sdk.ServiceDefaults
{
    public static class ServiceRegistration
    {
        private const string ClientName = "GatewayServiceClient";

        public static IServiceCollection AddGatewayServiceSdk(this IServiceCollection services, string serviceUrl, bool IsServiceAccount = false, IConfiguration? configuration = null)
        {

            services.AddSdkAuthentication(ClientName, serviceUrl, IsServiceAccount, configuration);


            services.AddScoped<IKiotaApiClient<GatewayClient>>(sp => new KiotaApiClientAbstraction<GatewayClient>(sp.GetRequiredService<IHttpClientFactory>(),
                                                                                                             ClientName,
                                                                                                             requestAdapter => new GatewayClient(requestAdapter)));
            services.AddScoped<IGatewayService, GatewayApiClientService>();
            return services;
        }
    }
}
