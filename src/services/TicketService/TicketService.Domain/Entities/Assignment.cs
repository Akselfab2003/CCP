namespace TicketService.Domain.Entities
{
    public class Assignment
    {
        public Guid Id { get; set; }
        public int TicketId { get; set; }
        public Guid UserId { get; set; }
        public Guid AssignByUserId { get; set; }
        public DateTime UpdatedAt { get; set; }

        public void AddRequiredInfo(int ticketId, Guid userId, Guid assignByUserId)
        {
            Id = Guid.NewGuid();
            TicketId = ticketId;
            UserId = userId;
            AssignByUserId = assignByUserId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateAssignment(Guid userId, Guid assignByUserId)
        {
            UserId = userId;
            AssignByUserId = assignByUserId;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
