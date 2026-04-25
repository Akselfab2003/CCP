using TestUtils.Integration;
using TicketService.Infrastructure.Persistence;
using TicketService.Infrastructure.ServiceCollection;
using TicketService.Sdk.ServiceDefaults;

namespace TicketService.Api.IntegrationTests.Fixtures
{
    public class TicketServiceFixture : GenericIntegrationSdkAndDbTestFixture<TicketDbContext>, IAsyncLifetime
    {
        public override string DBResourceName => "ticketdb";

        public override string APIResourceName => "ticketservice-api";
        public override List<string> RequiredResources =>
        [
            APIResourceName,
            DBResourceName,
            "keycloak",
            "postgres",
            "RabbitMQ",
            "emailservice-api",
            "emaildb",
            "customerdb",
            "customerservice-api"
        ];

        public async ValueTask InitializeAsync()
        {
            await Initialize();
            DB_Services.AddInfrastructure();
            SDK_Services.AddTicketServiceSdk(GetServiceUrl(APIResourceName), true);
            await BuildProviders();
        }

        public async ValueTask DisposeAsync()
        {
            await Dispose();
        }
    }
}
