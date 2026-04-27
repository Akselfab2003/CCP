using CCP.Sdk.utils.Abstractions;
using CCP.Sdk.utils.Authentication;
using ChatService.Sdk.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ChatService.Sdk.ServiceDefaults
{
    public static class ServiceRegistration
    {
        private readonly static string ChatServiceClientName = "ChatServiceClient";
        public static IServiceCollection AddChatServiceSdk(this IServiceCollection services, string serviceUrl, bool IsServiceAccount = false)
        {
            services.AddSdkAuthentication(ChatServiceClientName, serviceUrl, IsServiceAccount);


            services.AddScoped<IKiotaApiClient<ChatServiceClient>>(sp => new KiotaApiClientAbstraction<ChatServiceClient>(sp.GetRequiredService<IHttpClientFactory>(),
                                                                                                             ChatServiceClientName,
                                                                                                             requestAdapter => new ChatServiceClient(requestAdapter)));

            services.AddScoped<IFaqService, FaqServiceClient>()
                    .AddScoped<IChatService, ChatClient>()
                    .AddScoped<IDomainService, DomainServiceClient>();

            return services;
        }
    }
}
