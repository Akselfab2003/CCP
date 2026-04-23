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

        public DbSet<Ticket> Tickets => Set<Ticket>();
        public DbSet<Assignment> Assignments => Set<Assignment>();
        public DbSet<TicketHistoryEntry> TicketHistory => Set<TicketHistoryEntry>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Ticket>().HasQueryFilter(t => _currentUser.IsServiceAccount || t.OrganizationId == _currentUser.OrganizationId);
            modelBuilder.ApplyConfiguration<Ticket>(new TicketEntityConfiguration());
            modelBuilder.ApplyConfiguration<Assignment>(new AssignmentEntityConfiguration());
            modelBuilder.ApplyConfiguration<TicketHistoryEntry>(new TicketHistoryEntryConfiguration());
        }
    }
}
