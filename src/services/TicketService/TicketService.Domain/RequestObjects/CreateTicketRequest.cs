namespace TicketService.Domain.RequestObjects
{
    public class CreateTicketRequest
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public TicketOrigin Origin { get; set; } = TicketOrigin.Manual;
        public Guid? CustomerId { get; set; }
        public Guid? OrganizationId { get; set; } = Guid.Empty;
        public Guid? AssignedUserId { get; set; }
        public List<string> InternalNotes { get; set; } = [];
    }
}
