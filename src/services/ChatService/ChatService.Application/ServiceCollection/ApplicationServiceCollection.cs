using ChatService.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ChatService.Application.ServiceCollection
{
    public static class ApplicationServiceCollection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<ISessionManagement, SessionManagement>();
            return services;
        }
    }
}
