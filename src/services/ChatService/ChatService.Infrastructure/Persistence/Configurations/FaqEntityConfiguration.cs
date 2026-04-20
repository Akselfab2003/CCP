using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatService.Infrastructure.Persistence.Configurations
{
    public class FaqEntityConfiguration : IEntityTypeConfiguration<Domain.Entities.FaqEntity>
    {
        public void Configure(EntityTypeBuilder<Domain.Entities.FaqEntity> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Question)
                  .IsRequired()
                  .HasMaxLength(500);

            builder.Property(e => e.Answer)
                  .IsRequired()
                  .HasMaxLength(2000);

            builder.Property(e => e.Category)
                  .HasMaxLength(100);

            builder.Property(e => e.OrgId)
                   .IsRequired();

            builder.Property(e => e.CreatedAt)
                   .IsRequired();

            builder.Property(e => e.UpdatedAt)
                   .IsRequired();

            builder.Property(e => e.Embedding)
                  .HasColumnType("vector(768)")
                  .IsRequired();
        }
    }
}
