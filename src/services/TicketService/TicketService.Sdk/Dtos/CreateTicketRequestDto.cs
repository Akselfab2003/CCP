namespace TicketService.Sdk.Dtos
{
    public class CreateTicketRequestDto
    {
        public Guid? AssignedUserId { get; set; }
        public Guid CustomerId { get; set; }
        public required string Title { get; set; }
    }
}
