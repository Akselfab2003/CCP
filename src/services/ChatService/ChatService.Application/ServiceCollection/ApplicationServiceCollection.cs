using ChatService.Application.AuthContext;
using ChatService.Application.Services.Chat;
using ChatService.Application.Services.Domain;
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

            services.AddScoped<IActiveSession, ActiveSession>()
                    .AddScoped<IChatManagementService, ChatManagementService>()
                    .AddScoped<IDomainServices, DomainServices>()
                    .AddScoped<IAuthParser, AuthParser>();

            return services;
        }
    }
}
