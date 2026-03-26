using ChatApp.Shared.ValueObjects;

namespace TicketService.Domain.Entities
{
    public class Ticket
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public TicketStatus Status { get; set; }
        public Guid OrganizationId { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? AssignmentId { get; set; }
        public Assignment? Assignment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<string> InternalNotes { get; set; } = [];


        public void AddRequiredInfo(string title, Guid? customerId, Guid organizationId)
        {
            Title = title;
            OrganizationId = organizationId;
            CustomerId = customerId;
            CreatedAt = DateTime.UtcNow;
            InternalNotes = [];
            Status = TicketStatus.Open;
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
