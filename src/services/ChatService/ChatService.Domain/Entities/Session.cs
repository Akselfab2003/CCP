namespace ChatService.Domain.Entities
{
    public class Session
    {
        public Guid SessionId { get; set; }
        public Guid OrganizationId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
