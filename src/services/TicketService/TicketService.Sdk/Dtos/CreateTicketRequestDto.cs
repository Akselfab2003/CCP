namespace TicketService.Sdk.Dtos
{
    public class CreateTicketRequestDto
    {
        public Guid? AssignedUserId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid? OrganizationId { get; set; } = Guid.Empty;
        public required string Title { get; set; }
        public string? Description { get; set; }
    }
}
