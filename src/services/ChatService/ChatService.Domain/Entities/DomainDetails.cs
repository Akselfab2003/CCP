namespace ChatService.Domain.Entities
{
    public class DomainDetails
    {
        public Guid Id { get; set; }
        public Guid OrgId { get; set; }
        public string Domain { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
