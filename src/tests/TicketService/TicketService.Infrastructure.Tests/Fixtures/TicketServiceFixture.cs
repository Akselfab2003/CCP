using TestUtils.Integration;
using TicketService.Infrastructure.Persistence;
using TicketService.Infrastructure.ServiceCollection;

namespace TicketService.Infrastructure.Tests.Fixtures
{
    public class TicketServiceFixture : GenericIntegrationSdkAndDbTestFixture<TicketDbContext>, IAsyncLifetime
    {
        public override string DBResourceName => "ticketdb";

        public override string APIResourceName => "postgres";
        public override List<string> RequiredResources =>
        [
            "postgres",
            "RabbitMQ",
            DBResourceName
        ];

        public async ValueTask InitializeAsync()
        {
            DefaultTimeout = TimeSpan.FromSeconds(30);
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
