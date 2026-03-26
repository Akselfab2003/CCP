using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketService.Domain.Entities;

namespace TicketService.Infrastructure.Persistence.Configurations
{
    public class TicketEntityConfiguration : IEntityTypeConfiguration<Ticket>
    {
        public void Configure(EntityTypeBuilder<Ticket> builder)
        {
            builder.HasKey(t => t.Id);
            builder.Property(t => t.Title)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(t => t.Status)
                   .IsRequired();

            builder.Property(t => t.OrganizationId).IsRequired();

            builder.Property(t => t.CustomerId)
                   .IsRequired(false);

            builder.Property(t => t.InternalNotes)
                   .IsRequired();

            builder.Property(t => t.CreatedAt)
                   .IsRequired();

            builder.HasOne(t => t.Assignment)
                   .WithMany()
                   .HasForeignKey(t => t.AssignmentId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
