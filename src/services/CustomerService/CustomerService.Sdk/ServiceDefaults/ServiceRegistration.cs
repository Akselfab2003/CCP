using CCP.Sdk.utils.Abstractions;
using CCP.Sdk.utils.Authentication;
using CustomerService.Sdk.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerService.Sdk.ServiceDefaults
{
    // Extension methods for registering CustomerService SDK
    public static class ServiceRegistration
    {
        private const string ServiceName = "CustomerService";

        // Registers CustomerService SDK with Kiota client and authentication
        public static IServiceCollection AddCustomerviceSdk(this IServiceCollection services, string serviceUrl, bool isServiceAccount = false)
        {
            // Setup authentication for Customer API
            services.AddSdkAuthentication(ServiceName, serviceUrl, isServiceAccount);

            // Register Kiota API client for Customer Service
            services.AddScoped<IKiotaApiClient<CustomerServiceClient>>(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();

                return new KiotaApiClientAbstraction<CustomerServiceClient>(
                    httpClientFactory,
                    ServiceName,
                    requestAdapter => new CustomerServiceClient(requestAdapter));
            });

            // Register SDK service
            services.AddScoped<ICustomerSdkService, CustomerSdkService>();

            return services;
        }
    }
}
