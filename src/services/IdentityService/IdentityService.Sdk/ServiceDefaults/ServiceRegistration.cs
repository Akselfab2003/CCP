using CCP.Sdk.utils.Abstractions;
using CCP.Sdk.utils.Authentication;
using IdentityService.Sdk.Services.Customer;
using IdentityService.Sdk.Services.Group;
using IdentityService.Sdk.Services.Tenant;
using IdentityService.Sdk.Services.User;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.Sdk.ServiceDefaults
{
    public static class ServiceRegistration
    {
        private const string IdentityServiceClientName = "IdentityServiceClient";

        public static IServiceCollection AddIdentityServiceSdk(this IServiceCollection services, string serviceUrl, bool IsServiceAccount = false)
        {
            services.AddSdkAuthentication(IdentityServiceClientName, serviceUrl, IsServiceAccount);

            services.AddScoped<IKiotaApiClient<IdentityServiceClient>>(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                return new KiotaApiClientAbstraction<IdentityServiceClient>(httpClientFactory, IdentityServiceClientName, requestAdapter => new IdentityServiceClient(requestAdapter));
            });

            services.AddScoped<IGroupService, GroupServiceClient>()
                    .AddScoped<IUserService, UserServiceClient>()
                    .AddScoped<ITenantService, TenantServiceClient>()
                    .AddScoped<ICustomerService, CustomerServiceClient>();

            return services;
        }
    }
}
