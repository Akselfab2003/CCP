using CCP.Website.Services;
using IdentityService.Sdk.Services.Customer;
using IdentityService.Sdk.Services.Supporter;
using IdentityService.Sdk.Services.Tenant;
using IdentityService.Sdk.Services.User;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
namespace CPP.UI.Tests.Fixtures.Website
{
    public class TestFactory : IDisposable
    {
        private IHost _host = default!;
        public string BaseUrl { get; private set; } = default!;
        public void Start()
        {
            var builder = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webhost =>
                {
                    webhost.UseKestrel();
                    webhost.UseUrls("http://127.0.0.1:0"); // Use dynamic port allocation
                    webhost.UseEnvironment("Test");
                    webhost.ConfigureServices(services =>
                    {
                        ConfigureMocks(services);
                        ConfigureAuthentication(services);
                    });
                    webhost.UseStartup<CCP.Website.Program>();
                });

            _host = builder.Build();
            _host.StartAsync().Wait();

            var serverAddressesFeature = _host.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>();
            if (serverAddressesFeature == null || !serverAddressesFeature.Addresses.Any())
            {
                throw new InvalidOperationException("No server addresses were found.");
            }

            // Capture the first valid address
            BaseUrl = serverAddressesFeature.Addresses.First();
        }

        private void ConfigureMocks(IServiceCollection services)
        {
            services.RemoveService<IWebsiteReferencesService>();
            services.RemoveService<IUserService>();
            services.RemoveService<ITenantService>();
            services.RemoveService<ICustomerService>();
            services.RemoveService<ISupporterService>();

            services.AddMockedScoped<IWebsiteReferencesService>();
            services.AddMockedScoped<IUserService>();
            services.AddMockedScoped<ITenantService>();
            services.AddMockedScoped<ICustomerService>();
            services.AddMockedScoped<ISupporterService>();
        }

        private void ConfigureAuthentication(IServiceCollection services)
        {
            services.AddScoped<AuthenticationStateProvider, FakeAuthStateProvider>();
        }

        private void ConfigureConfiguration(IConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
            });
        }

        public void Dispose() => _host?.Dispose();
    }
}
