namespace TicketService.Domain.RequestObjects
{
    public class UpdateTicketStatusRequest
    {
        public TicketStatus NewStatus { get; set; }
    }
}
