using ChatService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatService.Infrastructure.Persistence.Configurations
{
    public class SessionEntityConfiguration : IEntityTypeConfiguration<SessionEntity>
    {
        public void Configure(EntityTypeBuilder<SessionEntity> builder)
        {
            builder.HasKey(e => e.SessionId);
            builder.Property(e => e.OrganizationId)
                   .IsRequired();
            builder.Property(e => e.CreatedAt)
                   .IsRequired();
            builder.Property(e => e.UpdatedAt)
                   .IsRequired();
        }
    }
}
