namespace TicketService.Domain.RequestObjects
{
    public class CreateTicketRequest
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? AssignedUserId { get; set; }
        public List<string> InternalNotes { get; set; } = [];
    }
}
