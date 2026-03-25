using Aspire.Hosting.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace TestUtils.Integration
{
    public abstract class GenericIntegrationSdkAndDbTestFixture<DBContext> : GenericIntegrationTestFixture where DBContext : DbContext
    {
        public abstract string DBResourceName { get; }
        public IServiceProvider DB => DB_Provider;

        private IServiceProvider DB_Provider = null!;

        public IServiceCollection DB_Services = new ServiceCollection();

        public override async Task Initialize()
        {
            await base.Initialize();
            await InitializeDB();
        }

        public override async Task BuildProviders()
        {
            await base.BuildProviders();
            DB_Provider = DB_Services.BuildServiceProvider();

            // Apply migrations
            using var scope = DB.CreateScope();
            DBContext dbContext = scope.ServiceProvider.GetRequiredService<DBContext>();
            await dbContext.Database.MigrateAsync();
        }

        private async Task InitializeDB()
        {
            var ConnectionString = await App.GetConnectionStringAsync(DBResourceName);
            DB_Services.AddLogging();
            DB_Services.AddDbContext<DBContext>(options =>
            {
                options.UseNpgsql(ConnectionString, op => op.UseVector());
            });
        }
    }
}
