namespace TicketService.Domain.RequestObjects
{
    public class CreateTicketRequest
    {
        public required string Title { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? OrganizationId { get; set; } = Guid.Empty;
        public Guid? AssignedUserId { get; set; }
        public List<string> InternalNotes { get; set; } = [];
    }
}
