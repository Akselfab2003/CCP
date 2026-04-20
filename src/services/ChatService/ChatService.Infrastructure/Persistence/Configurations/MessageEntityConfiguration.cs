using ChatService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatService.Infrastructure.Persistence.Configurations
{
    public class MessageEntityConfiguration : IEntityTypeConfiguration<MessageEntity>
    {
        public void Configure(EntityTypeBuilder<MessageEntity> builder)
        {

            builder.HasKey(e => e.Id);

            builder.Property(e => e.OrgId)
                .IsRequired();

            builder.Property(e => e.ConversationId)
                .IsRequired();

            builder.Property(e => e.IsFromUser)
                .IsRequired();

            builder.Property(e => e.Message)
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                .IsRequired();
        }
    }
}
