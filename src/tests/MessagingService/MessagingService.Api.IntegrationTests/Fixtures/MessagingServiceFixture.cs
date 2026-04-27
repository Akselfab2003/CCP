using CCP.Shared.AuthContext;
using Duende.AccessTokenManagement;
using Duende.IdentityModel.Client;
using MessagingService.Application.Services;
using MessagingService.Domain.Interfaces;
using MessagingService.Infrastructure.Persistence;
using MessagingService.Sdk.ServiceDefaults;
using Microsoft.Extensions.Configuration;
using TestUtils.Integration;
using TicketService.Sdk.ServiceDefaults;

namespace MessagingService.Api.IntegrationTests.Fixtures
{
    public class MessagingServiceFixture : GenericIntegrationSdkAndDbTestFixture<MessagingDbContext>, IAsyncLifetime
    {
        public override string APIResourceName => "messagingservice-api";
        public override string DBResourceName => "MessagingDatabase";
        public override List<string> RequiredResources => [APIResourceName, DBResourceName, "keycloak", "postgres", "MailHog", "RabbitMQ", "ticketservice-api", "ticketdb", "emailservice-api", "emaildb", "customerdb", "customerservice-api", "identityservice-api", "chatDB", "ollama", "chatservice-api"];

        private IServiceCollection TicketSdk = null!;
        public IServiceProvider TicketSDK = null!;

        public async ValueTask InitializeAsync()
        {
            IsRemoveNotNeededResourcesForTestingEnabled = true;
            DefaultTimeout = TimeSpan.FromMinutes(1);
            await Initialize();
            DB_Services.AddScoped<IMessageService, MessageService>();
            var serviceUrl = GetServiceUrl(APIResourceName);
            SDK_Services.AddHttpContextAccessor();
            SDK_Services.AddSingleton<ServiceAccountOverrider>();
            SDK_Services.AddMessageServiceSDK(serviceUrl, true);
            TicketSdk = new ServiceCollection();
            await InitializeSDK();
            TicketSDK = TicketSdk
                           .AddTicketServiceSdk(GetServiceUrl("ticketservice-api"), true)
                           .AddSingleton<ServiceAccountOverrider>()
                           .BuildServiceProvider();

            await BuildProviders();
        }
        private async Task InitializeSDK()
        {
            IConfiguration configuration = GetConfiguration();

            TicketSdk.AddLogging();

            // Configure OAuth2 client credentials for authentication
            TicketSdk.AddClientCredentialsTokenManagement()
                        .AddClient(ClientCredentialsClientName.Parse("CCP.ServiceAccount"), client =>
                        {
                            client.TokenEndpoint = new Uri("http://localhost:8080/realms/CCP/protocol/openid-connect/token");
                            client.ClientId = ClientId.Parse("CCP.ServiceAccount");
                            client.ClientSecret = ClientSecret.Parse(
                                configuration["SERVICE_ACCOUNT_SECRET"]
                                ?? throw new InvalidOperationException("SERVICE_ACCOUNT_SECRET configuration value is required.")
                            );
                            client.Scope = Scope.ParseOrDefault("openid");
                            client.ClientCredentialStyle = ClientCredentialStyle.AuthorizationHeader;
                        });
        }

        public async ValueTask DisposeAsync()
        {
            await Dispose();
        }
    }
}
