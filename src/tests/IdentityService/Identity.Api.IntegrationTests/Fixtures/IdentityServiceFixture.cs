using Duende.AccessTokenManagement;
using Duende.IdentityModel.Client;
using IdentityService.Sdk.ServiceDefaults;
using Keycloak.Sdk.ServiceDefaults;
using Microsoft.Extensions.Configuration;
using TestUtils.Integration;

namespace Identity.Api.IntegrationTests.Fixtures
{
    public class IdentityServiceFixture : GenericIntegrationTestFixture, IAsyncLifetime
    {

        private IServiceProvider keycloak_SDK = null!;
        public IServiceProvider Keycloak_SDK => keycloak_SDK;

        public override List<string> RequiredResources =>
        [
            "MailHog",
            "identityservice-api",
            "keycloak",
        ];

        public override string APIResourceName => "identityservice-api";

        public async ValueTask InitializeAsync()
        {
            await Initialize();
            SDK_Services.AddIdentityServiceSdk(GetServiceUrl(APIResourceName), true);
            await InitializeKeyCloakSDK();
            await BuildProviders();
        }

        private async Task InitializeKeyCloakSDK()
        {
            var ct = TestContext.Current.CancellationToken;
            var Keycloak = GetServiceUrl("keycloak");
            IConfiguration configuration = GetConfiguration();

            var services = new ServiceCollection();
            services.AddLogging();

            services.AddClientCredentialsTokenManagement()
                   .AddClient(ClientCredentialsClientName.Parse("KeyCloak.Admin"), client =>
                   {
                       client.TokenEndpoint = new Uri("http://localhost:8080/realms/CCP/protocol/openid-connect/token");
                       client.ClientId = ClientId.Parse("KeycloakAdminApiClient");
                       client.ClientSecret = ClientSecret.Parse(configuration["KeycloakAdminApiClientSecret"] ?? throw new InvalidOperationException("KeycloakAdminApiClientSecret configuration value is required."));
                       client.Scope = Scope.ParseOrDefault("openid");
                       client.ClientCredentialStyle = ClientCredentialStyle.AuthorizationHeader;
                   });

            services.AddKeycloakSdk(Keycloak);
            keycloak_SDK = services.BuildServiceProvider();
        }

        public async ValueTask DisposeAsync()
        {
            await Dispose();
        }
    }
}
