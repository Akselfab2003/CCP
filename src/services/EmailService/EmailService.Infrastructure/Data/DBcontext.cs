using CCP.Shared.AuthContext;
using EmailService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EmailService.Infrastructure.Data
{
    public class DBcontext : DbContext
    {
        private readonly ICurrentUser _currentUser;
        public DBcontext(DbContextOptions<DBcontext> options) : base(options)
        {
            _currentUser = this.GetService<ICurrentUser>();
        }

        public DbSet<EmailSent> EmailSent { get; set; }
        public DbSet<EmailReceived> EmailReceived { get; set; }
        public DbSet<TenantEmailConfiguration> TenantEmailConfigurations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TenantEmailConfiguration>().HasQueryFilter(t => t.OrganizationId == _currentUser.OrganizationId);
        }
    }
}
