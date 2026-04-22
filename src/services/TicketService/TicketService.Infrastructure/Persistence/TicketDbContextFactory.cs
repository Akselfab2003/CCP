using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace TicketService.Infrastructure.Persistence
{
    public class TicketDbContextFactory : IDesignTimeDbContextFactory<TicketDbContext>
    {
        public TicketDbContext CreateDbContext(string[] args)
        {
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            services.AddScoped<ICurrentUser, DesignTimeCurrentUser>();

            var optionsBuilder = new DbContextOptionsBuilder<TicketDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Database=ticketdb;Username=postgres;Password=postgres");
            optionsBuilder.UseApplicationServiceProvider(services.BuildServiceProvider());

            return new TicketDbContext(optionsBuilder.Options);
        }

        private class DesignTimeCurrentUser : ICurrentUser
        {
            public Guid UserId => Guid.Empty;
            public Guid OrganizationId => Guid.Empty;
            public string OrganizationName => string.Empty;
            public void SetCurrentUser(Guid userId) { }
            public void SetOrganizationId(Guid organizationId) { }
            public void SetOrganizationName(string organizationName) { }
        }
    }
}
