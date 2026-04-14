using CCP.Website.Services;
using IdentityService.Sdk.Services.Customer;
using IdentityService.Sdk.Services.Supporter;
using IdentityService.Sdk.Services.Tenant;
using IdentityService.Sdk.Services.User;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
namespace CPP.UI.Tests.Fixtures.Website
{
    public class TestFactory : WebApplicationFactory<CCP.Website.Program>
    {
        public string BaseUrl { get; private set; } = default!;

        private readonly Dictionary<System.Type, object> _mockproviders = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development"); // Set the environment to Test
            builder.UseConfiguration(new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["services:Keycloak:http:0"] = "http://localhost:8080",
                    ["services:ccp-ui:https:0"] = "http://localhost:5000",
                    ["services:identityservice-api:https:0"] = "http://localhost:5001",
                    ["CircuitOptions.DetailedErrors"] = "true",
                })
                .Build());
            builder.ConfigureServices(services =>
            {
                services.AddFluentUIComponents();

                ConfigureMocks(services);
                ConfigureAuth(services);
            });
        }


        private void ConfigureMocks(IServiceCollection services)
        {
            AddMockedScoped<IWebsiteReferencesService>(services);
            AddMockedScoped<IUserService>(services);
            AddMockedScoped<ITenantService>(services);
            AddMockedScoped<ICustomerService>(services);
            AddMockedScoped<ISupporterService>(services);
        }

        private void AddMockedScoped<T>(IServiceCollection services)
            where T : class
        {
            RemoveService<T>(services);

            var provider = new MockProvider<T>();
            _mockproviders[typeof(T)] = provider;

            services.AddSingleton(provider);

            services.AddScoped(sp =>
            {
                var mock = provider.Current;

                if (mock == null)
                    throw new InvalidOperationException(
                        $"Mock for {typeof(T).Name} not configured.");

                return mock;
            });
        }

        private void RemoveService<T>(IServiceCollection services)
        {
            var descriptor = services.FirstOrDefault(
                d => d.ServiceType == typeof(T));

            if (descriptor != null)
                services.Remove(descriptor);
        }

        public T SetMock<T>() where T : class
        {
            var mock = NSubstitute.Substitute.For<T>();
            ((MockProvider<T>)_mockproviders[typeof(T)]).Current = mock;
            return mock;
        }

        public void ResetMocks()
        {
            foreach (var provider in _mockproviders.Values)
            {
                ((dynamic)provider).ClearReceivedCalls();
            }
        }

        // -------------------------
        // Auth
        // -------------------------
        private void ConfigureAuth(IServiceCollection services)
        {
            services.AddAuthorizationCore();

            services.AddScoped<
                Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider,
                FakeAuthStateProvider>();
        }
    }
}
