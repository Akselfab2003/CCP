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
            services.AddScoped<IGatewayService, GatewayApiClientService>();
            return services;
        }
    }
}
