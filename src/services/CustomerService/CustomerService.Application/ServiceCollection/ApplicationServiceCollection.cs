using Microsoft.Extensions.DependencyInjection;

namespace CustomerService.Application.ServiceCollection
{
    public static class ApplicationServiceCollection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<Services.ICustomerService, Services.CustomerService>();
            return services;
        }
    }
}
