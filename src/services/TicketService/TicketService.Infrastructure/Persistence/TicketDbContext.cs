using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using TicketService.Domain.Entities;
using TicketService.Infrastructure.Persistence.Configurations;

namespace TicketService.Infrastructure.Persistence
{
    public class TicketDbContext : DbContext
    {
        private readonly ICurrentUser _currentUser;
        private bool _IsDesignTime;
        public TicketDbContext(DbContextOptions<TicketDbContext> options) : base(options)
        {
            _currentUser = this.GetService<ICurrentUser>();
        }

        public DbSet<Domain.Entities.Ticket> Tickets => Set<Domain.Entities.Ticket>();
        public DbSet<Domain.Entities.Assignment> Assignments => Set<Domain.Entities.Assignment>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            _IsDesignTime = !optionsBuilder.IsConfigured;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (_IsDesignTime)
            {
                modelBuilder.ApplyConfiguration<Ticket>(new TicketEntityConfiguration());
                modelBuilder.ApplyConfiguration<Assignment>(new AssignmentEntityConfiguration());
                return;
            }
            else
            {
                modelBuilder.ApplyConfiguration<Ticket>(new TicketEntityConfiguration());
                modelBuilder.ApplyConfiguration<Assignment>(new AssignmentEntityConfiguration());
                modelBuilder.Entity<Ticket>().HasQueryFilter(t => t.OrganizationId == _currentUser.OrganizationId);
            }
        }
    }
}
