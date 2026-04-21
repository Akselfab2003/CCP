using ChatService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatService.Infrastructure.Persistence.Configurations
{
    public class DomainDetailsConfiguration : IEntityTypeConfiguration<DomainDetails>
    {
        public void Configure(EntityTypeBuilder<DomainDetails> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.OrgId)
                .IsRequired();

            builder.Property(x => x.Domain)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();
        }
    }
}
