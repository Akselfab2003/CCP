using CCP.Shared.AuthContext;
using CCP.Shared.UIContext;
using CCP.UI.Services;
using CPP.UI.Tests.Fixtures.Website;
using IdentityService.Sdk.Services.Customer;
using IdentityService.Sdk.Services.Supporter;
using IdentityService.Sdk.Services.Tenant;
using IdentityService.Sdk.Services.User;
using MessagingService.Sdk.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;
using TicketService.Sdk.Services.Assignment;
using TicketService.Sdk.Services.Ticket;
namespace CPP.UI.Tests.Fixtures.Application
{
    public class TestFactoryApplication : WebApplicationFactory<CCP.UI.Program>
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
                    ["services:identityservice-api:http:0"] = "http://localhost:5001",
                    ["services:messagingservice-api:http:0"] = "http://localhost:5002",
                    ["services:ticketservice-api:http:0"] = "http://localhost:5003",
                    ["services:customerservice-api:http:0"] = "http://localhost:5004",
                    ["services:messagingservice-api:http:0"] = "http://localhost:5005",
                    ["services:EmailService:http:0"] = "http://localhost:5006",
                    ["CircuitOptions.DetailedErrors"] = "true",
                    ["UI_TESTS"] = "true"
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
            AddMockedScoped<ChatHubService>(services);
            AddMockedScoped<ICurrentUser>(services);
            AddMockedScoped<IUIUserContext>(services);

            // MessagingService Sdk
            AddMockedScoped<IMessageSdkService>(services);

            // IdentityService Sdk
            AddMockedScoped<IUserService>(services);
            AddMockedScoped<ITenantService>(services);
            AddMockedScoped<ICustomerService>(services);
            AddMockedScoped<ISupporterService>(services);

            // TicketService Sdk
            AddMockedScoped<ITicketService>(services);
            AddMockedScoped<IAssignmentService>(services);
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
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = FakeAuthStateProvider.Scheme;
                options.DefaultChallengeScheme = FakeAuthStateProvider.Scheme;
            }).AddScheme<AuthenticationSchemeOptions, FakeAuthStateProvider>(FakeAuthStateProvider.Scheme, options => { });

            services.AddAuthorization();
        }
    }
}
