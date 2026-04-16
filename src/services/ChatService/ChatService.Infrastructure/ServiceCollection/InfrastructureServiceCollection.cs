using ChatService.Application.Interfaces;
using ChatService.Domain.Interfaces;
using ChatService.Infrastructure.LLM.Embedding;
using ChatService.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChatService.Infrastructure.ServiceCollection
{
    public static class InfrastructureServiceCollection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IFaqRepository, FaqRepository>()
                    .AddScoped<ISessionRepository, SessionRepository>();

            services.AddScoped<IEmbeddingService, EmbeddingService>();


            return services;
        }
    }
}
