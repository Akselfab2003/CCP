using System;
using System.Collections.Generic;
using System.Text;
using CCP.Sdk.utils.Abstractions;
using CCP.Sdk.utils.Authentication;
using EmailService.Sdk.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EmailService.Sdk.ServiceDefaults
{
    public static class ServiceRegistation
    {
        private const string ServiceName = "EmailService";

        public static IServiceCollection AddEmailServiceSdk(this IServiceCollection services, string serviceUrl, bool IsServiceAccount = false)
        {
            services.AddSdkAuthentication(ServiceName, serviceUrl, IsServiceAccount);
            services.AddScoped<IKiotaApiClient<EmailServiceClient>>(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
               return new KiotaApiClientAbstraction<EmailServiceClient>(httpClientFactory, ServiceName, requestAdapter => new EmailServiceClient(requestAdapter));
            });

            //Register IEmailService using the named HttpClient created by AddSdkAuthentication so BaseAddress and auth handlers are applied
            services.AddScoped<IEmailService, EmailService.Sdk.Services.EmailService>();
            return services;
        }
    }
}
