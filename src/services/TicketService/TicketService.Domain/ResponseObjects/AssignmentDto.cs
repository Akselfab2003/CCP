namespace TicketService.Domain.ResponseObjects
{
    public class AssignmentDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid AssignedByUserId { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
