using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketService.Domain.Entities;

namespace TicketService.Infrastructure.Persistence.Configurations
{
    public class TicketHistoryEntryConfiguration : IEntityTypeConfiguration<TicketHistoryEntry>
    {
        public void Configure(EntityTypeBuilder<TicketHistoryEntry> builder)
        {
            builder.ToTable("ticket_history");

            builder.HasKey(h => h.Id);

            builder.Property(h => h.EventType)
                   .IsRequired()
                   .HasMaxLength(64);

            builder.Property(h => h.OldValue)
                   .HasMaxLength(256);

            builder.Property(h => h.NewValue)
                   .HasMaxLength(256);

            builder.Property(h => h.OccurredAt)
                   .IsRequired();

            builder.HasOne<Ticket>()
                   .WithMany()
                   .HasForeignKey(h => h.TicketId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
