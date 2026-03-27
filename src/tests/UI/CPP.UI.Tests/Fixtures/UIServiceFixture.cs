using IdentityService.Sdk.ServiceDefaults;
using TestUtils.Integration;

namespace CPP.UI.Tests.Fixtures
{
    public class UIServiceFixture : GenericIntegrationTestFixture, IAsyncLifetime
    {
        public string UIEndpoint => GetServiceUrl("ccp-ui", "https");
        public string WebsiteEndpoint => GetServiceUrl("ccp-website", "https");
        public override List<string> RequiredResources =>
        [
            APIResourceName,
            "keycloak",
            "postgres",
            "ollama",
            "Roundcube",
            "emaildb",
            "chatdb",
            "customerdb",
            "MessagingDatabase",
            "chatdb",
            "ticketdb",
            "emailservice",
            "identityservice-api",
            "chatapp-messagingservice",
            "customerservice-api",
            "MailServer"
        ];

        public override string APIResourceName => "ccp-ui";

        public async ValueTask InitializeAsync()
        {
            DefaultTimeout = TimeSpan.FromMinutes(5);
            IsRemoveNotNeededResourcesForTestingEnabled = false;
            await Initialize();
            var serviceUrl = GetServiceUrl("identityservice-api");
            SDK_Services.AddIdentityServiceSdk(serviceUrl, true);
            await BuildProviders();
        }
        public async ValueTask DisposeAsync()
        {
            await Dispose();
        }
    }
}
