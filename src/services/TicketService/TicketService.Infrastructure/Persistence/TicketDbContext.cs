using ChatApp.Shared.AuthContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TicketService.Domain.Entities;
using TicketService.Infrastructure.Persistence.Configurations;

namespace TicketService.Infrastructure.Persistence
{
    public class TicketDbContext : DbContext
    {
        private readonly ICurrentUser _currentUser;
        public TicketDbContext(DbContextOptions<TicketDbContext> options) : base(options)
        {
            _currentUser = this.GetService<ICurrentUser>();
        }

        public DbSet<Domain.Entities.Ticket> Tickets => Set<Domain.Entities.Ticket>();
        public DbSet<Domain.Entities.Assignment> Assignments => Set<Domain.Entities.Assignment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration<Ticket>(new TicketEntityConfiguration());
            modelBuilder.ApplyConfiguration<Assignment>(new AssignmentEntityConfiguration());
            if (_currentUser != null)
                modelBuilder.Entity<Ticket>().HasQueryFilter(t => t.OrganizationId == _currentUser.OrganizationId);
        }
    }
}
