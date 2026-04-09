using TestUtils.Integration;
using TicketService.Infrastructure.Persistence;
using TicketService.Infrastructure.ServiceCollection;

namespace TicketService.Infrastructure.Tests.Fixtures
{
    public class TicketServiceFixture : GenericIntegrationSdkAndDbTestFixture<TicketDbContext>, IAsyncLifetime
    {
        public override string DBResourceName => "ticketdb";

        public override string APIResourceName => "ticketservice-api";
        public override List<string> RequiredResources =>
        [
            APIResourceName,
            "keycloak",
            "postgres",
            DBResourceName
        ];

        public async ValueTask InitializeAsync()
        {
            await Initialize();
            DB_Services.AddInfrastructure();
            await BuildProviders();
        }

        public async ValueTask DisposeAsync()
        {
            await Dispose();
        }
    }
}
