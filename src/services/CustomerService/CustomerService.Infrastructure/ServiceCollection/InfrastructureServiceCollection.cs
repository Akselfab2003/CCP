using ChatApp.Encryption;
using CustomerService.Application.Persistence;
using CustomerService.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerService.Infrastructure.ServiceCollection
{
    public static class InfrastructureServiceCollection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {

            // Læs encryption key fra configuration (skal være 256-bit base64 encoded)
            var encryptionKey = configuration["Encryption_Key"]
                ?? throw new InvalidOperationException("Encryption_Key configuration value is required.");

            // Register repositories og encryption service
            services.AddScoped<ICustomerRepository, CustomerRepository>()
                    .AddSingleton<IEncryptionService>(new AesEncryptionService(encryptionKey));

            return services;
        }
    }
}
