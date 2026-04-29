using ChatService.Domain.Entities.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatService.Infrastructure.Persistence.Configurations
{
    public class GeneratedReplyConfiguration : IEntityTypeConfiguration<GeneratedReply>
    {
        public void Configure(EntityTypeBuilder<GeneratedReply> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.OrgId)
                .IsRequired();

            builder.Property(x => x.TicketId)
                .IsRequired();

            builder.Property(x => x.Reply)
                .IsRequired();

            builder.Property(x => x.Confidence)
                .IsRequired();

            builder.Property(x => x.Reasoning)
                .IsRequired();

            builder.Property(x => x.NeedsMoreInfo)
                .IsRequired();

            builder.Property(x => x.AlternativeReply)
                .IsRequired(false);

            builder.Property(x => x.AgentHeadsUp)
                .IsRequired(false);

            builder.Property(x => x.InfoNeeded)
                .IsRequired(false);
        }
    }
}
