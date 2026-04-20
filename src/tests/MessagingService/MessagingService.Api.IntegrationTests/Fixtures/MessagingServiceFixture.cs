using MessagingService.Application.Services;
using MessagingService.Domain.Interfaces;
using MessagingService.Infrastructure.Persistence;
using MessagingService.Sdk.ServiceDefaults;
using TestUtils.Integration;

namespace MessagingService.Api.IntegrationTests.Fixtures
{
    public class MessagingServiceFixture : GenericIntegrationSdkAndDbTestFixture<MessagingDbContext>, IAsyncLifetime
    {
        public override string APIResourceName => "messagingservice-api";
        public override string DBResourceName => "MessagingDatabase";
        public override List<string> RequiredResources => [APIResourceName, DBResourceName, "keycloak", "postgres", "MailHog", "RabbitMQ", "ticketservice-api", "ticketdb"];

        public async ValueTask InitializeAsync()
        {
            await Initialize();
            DB_Services.AddScoped<IMessageService, MessageService>();
            var serviceUrl = GetServiceUrl(APIResourceName);
            SDK_Services.AddMessageServiceSDK(serviceUrl, true);
            await BuildProviders();
        }

        public async ValueTask DisposeAsync()
        {
            await Dispose();
        }
    }
}
