namespace TicketService.Domain.ResponseObjects
{
    public class TicketDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public TicketStatus Status { get; set; }
        public TicketOrigin Origin { get; set; }
        public Guid OrganizationId { get; set; }
        public AssignmentDto? Assignment { get; set; }
        public Guid? CustomerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> InternalNotes { get; set; } = [];
    }

}
