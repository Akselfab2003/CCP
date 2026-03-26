using MessagingService.Application.Services;
using MessagingService.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace MessagingService.Application.ServiceCollection
{
    public static class ApplicationServiceCollection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IMessageService, MessageService>()
                    .AddScoped<IMessageAccessValidator, AllowAllMessageAccessValidator>();

            return services;
        }
    }
}
