using ChatApp.Encryption;
using EmailService.Application.Interfaces;
using EmailService.Domain.Interfaces;
using EmailService.Infrastructure.EmailInfrastructure;
using EmailTemplates.Renderes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmailService.Infrastructure.ServiceDefaults
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            var encryptionKey = configuration["Encryption_Key"]
                ?? throw new InvalidOperationException("Encryption_Key configuration value is required.");

            services.AddSingleton<IEncryptionService>(new AesEncryptionService(encryptionKey));

            services.AddScoped<IEmailReceived, EmailReceivedRepo>();
            services.AddScoped<IEmailSent, EmailSentRepo>();
            services.AddScoped<IEmail, EmailSendingService>();
            services.AddScoped<ISmtpClient, SmtpClient>();
            services.AddScoped<ITenantEmailConfigurationRepo, TenantEmailConfigurationRepo>();
            services.AddScoped<ITicketEmailService, TicketEmailService>();
            services.AddScoped<IEmailTicketMessageRepository, EmailTicketMessageRepository>();
            services.AddScoped<IEmailTemplateRenderer, EmailTemplateRenderer>();

            return services;
        }
    }
}
