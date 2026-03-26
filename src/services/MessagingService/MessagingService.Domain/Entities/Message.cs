using System.ComponentModel.DataAnnotations.Schema;
using Pgvector;

namespace MessagingService.Domain.Entities
{
    public class Message
    {
        public int Id { get; set; }

        public int TicketId { get; set; }

        public Guid? UserId { get; set; }
        public Guid OrganizationId { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAtUtc { get; set; }

        public bool IsEdited { get; set; }

        public bool IsDeleted { get; set; }
        public bool IsInternalNote { get; set; }

        public DateTime? DeletedAtUtc { get; set; }

        [Column(TypeName = "vector(1536)")]
        public Vector? Embedding { get; set; }
    }
}
