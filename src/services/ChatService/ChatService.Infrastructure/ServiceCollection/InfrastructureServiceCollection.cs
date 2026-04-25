using ChatService.Application.Interfaces;
using ChatService.Domain.Interfaces;
using ChatService.Infrastructure.LLM.Analysis;
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
                    .AddScoped<ISessionRepository, SessionRepository>()
                    .AddScoped<IConversationRepository, ConversationRepository>()
                    .AddScoped<IMessageRepository, MessageRepository>()
                    .AddScoped<IDomainDetailsRepository, DomainDetailsRepository>();

            services.AddScoped<IEmbeddingService, EmbeddingService>()
                    .AddScoped<ITicketAnalysisService, TicketAnalysisService>()
                    .AddScoped<IChatService, LLM.Chat.ChatService>()
                    .AddScoped<ITicketEmbeddingRepository, TicketEmbeddingRepository>()
                    .AddScoped<ITicketEmbeddingOrchestrator, TicketEmbeddingOrchestrator>();

            return services;
        }
    }
}
