using ChatService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatService.Infrastructure.Persistence.Configurations
{
    public class ConversationEntityConfiguration : IEntityTypeConfiguration<ConversationEntity>
    {
        public void Configure(EntityTypeBuilder<ConversationEntity> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.SessionId)
                   .IsRequired();

            builder.Property(p => p.OrgId)
                   .IsRequired();

            builder.Property(p => p.CreatedAt)
                   .IsRequired();

            builder.HasMany(p => p.Messages)
                   .WithOne()
                   .HasForeignKey(m => m.ConversationId);
        }
    }
}
