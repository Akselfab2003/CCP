namespace TicketService.Sdk.Dtos
{
    public class TicketSdkDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int Status { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid? CustomerId { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }

        // Assignment info (flattened for convenience)
        public Guid? AssignedUserId { get; set; }
        public Guid? AssignedByUserId { get; set; }
    }
}
