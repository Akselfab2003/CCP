namespace TicketService.Domain.Entities
{
    public class Ticket
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; private set; }
        public TicketStatus Status { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? AssignmentId { get; set; }
        public Assignment? Assignment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<string> InternalNotes { get; set; } = [];


        public void AddRequiredInfo(string title, Guid? customerId, Guid organizationId, string? description = null)
        {
            Title = title;
            OrganizationId = organizationId;
            CustomerId = customerId;
            CreatedAt = DateTime.UtcNow;
            InternalNotes = [];
            Status = TicketStatus.Open;
            Description = description;
        }

        public void UpdateDescription(string? description)
        {
            Description = description;
        }

        public void AddInternalNote(string note)
        {
            InternalNotes.Add(note);
        }

        public void RemoveInternalNote(string note)
        {
            InternalNotes.Remove(note);
        }

        public void UpdateStatus(TicketStatus newStatus)
        {
            Status = newStatus;
        }

        public void UpdateAssignmentReference(Guid assignmentId)
        {
            AssignmentId = assignmentId;
        }
    }
}
