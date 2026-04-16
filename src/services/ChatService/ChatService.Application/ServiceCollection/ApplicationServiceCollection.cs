using ChatService.Application.Services.Faq;
using ChatService.Application.Services.Session;
using Microsoft.Extensions.DependencyInjection;

namespace ChatService.Application.ServiceCollection
{
    public static class ApplicationServiceCollection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<ISessionManagement, SessionManagement>()
                    .AddScoped<IFaqManagementService, FaqManagementService>();
            return services;
        }
    }
}
