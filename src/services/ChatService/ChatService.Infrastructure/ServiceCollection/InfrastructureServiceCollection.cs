using ChatService.Domain.Interfaces;
using ChatService.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChatService.Infrastructure.ServiceCollection
{
    public class InfrastructureServiceCollection
    {
        public static void AddInfrastructureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IFaqRepository, FaqRepository>()
                    .AddScoped<ISessionRepository, SessionRepository>();
        }
    }
}
