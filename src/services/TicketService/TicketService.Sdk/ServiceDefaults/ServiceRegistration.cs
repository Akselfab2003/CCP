using Microsoft.Extensions.DependencyInjection;
using OrderFlow.Sdk.Utils.Abstractions;
using OrderFlow.Sdk.Utils.Authentication;
using TicketService.Sdk.Services.Assignment;
using TicketService.Sdk.Services.Ticket;
using TicketService.Sdk.Services.TicketSdk;

namespace TicketService.Sdk.ServiceDefaults
{
    public static class ServiceRegistration
    {
        private const string TicketServiceClientName = "TicketServiceClient";

        public static IServiceCollection AddTicketServiceSdk(this IServiceCollection services, string serviceUrl, bool IsServiceAccount = false)
        {
            services.AddSdkAuthentication(TicketServiceClientName, serviceUrl, IsServiceAccount);


            services.AddScoped<IKiotaApiClient<TicketServiceClient>>(sp => new KiotaApiClientAbstraction<TicketServiceClient>(sp.GetRequiredService<IHttpClientFactory>(),
                                                                                                             TicketServiceClientName,
                                                                                                             requestAdapter => new TicketServiceClient(requestAdapter)));

            services.AddScoped<ITicketService, TicketApiClientService>()
                    .AddScoped<IAssignmentService, AssignmentApiService>()
                    .AddScoped<ITicketSdkService, TicketSdkService>();


            return services;
        }
    }
}
