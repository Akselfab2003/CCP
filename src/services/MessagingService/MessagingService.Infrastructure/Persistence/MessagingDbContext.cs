using MessagingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MessagingService.Infrastructure.Persistence
{
    public class MessagingDbContext : DbContext
    {
        public MessagingDbContext(DbContextOptions<MessagingDbContext> options)
            : base(options)
        {
        }

        public DbSet<Message> Messages => Set<Message>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasPostgresExtension("vector");

            modelBuilder.Entity<Message>(entity =>
            {
                entity.HasKey(m => m.Id);

                entity.Property(m => m.TicketId).IsRequired();
                entity.Property(m => m.OrganizationId).IsRequired();
                entity.Property(m => m.UserId).IsRequired(false);

                entity.Property(m => m.Content)
                    .IsRequired()
                    .HasMaxLength(2000);

                entity.Property(m => m.CreatedAtUtc).IsRequired();
                entity.Property(m => m.UpdatedAtUtc);
                entity.Property(m => m.DeletedAtUtc);

                entity.Property(m => m.IsEdited).IsRequired();
                entity.Property(m => m.IsDeleted).IsRequired();

                entity.Property(m => m.IsInternalNote).IsRequired();

                entity.Property(m => m.Embedding)
                    .HasColumnType("vector(1536)");
            });
        }
    }
}
