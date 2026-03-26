using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketService.Domain.Entities;

namespace TicketService.Infrastructure.Persistence.Configurations
{
    public class AssignmentEntityConfiguration : IEntityTypeConfiguration<Assignment>
    {
        public void Configure(EntityTypeBuilder<Assignment> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.UserId)
                   .IsRequired();

            builder.Property(a => a.AssignByUserId)
                   .IsRequired();

            builder.HasOne<Ticket>()
                   .WithOne(t => t.Assignment)
                   .HasForeignKey<Assignment>(a => a.TicketId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(a => a.UpdatedAt)
                   .IsRequired();

        }
    }
}
