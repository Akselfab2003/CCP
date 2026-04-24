using MessagingService.Application.Interfaces;
using MessagingService.Infrastructure.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace MessagingService.Infrastructure.ServiceCollection
{
    public static class InfrastructureServiceCollection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IAttachmentStorageService, FileSystemAttachmentStorageService>();
            return services;
        }
    }
}
