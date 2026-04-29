namespace Gateway.Sdk.Dtos
{
    public class TicketDetailAggregateSdkDto
    {
        public TicketService.Sdk.Dtos.TicketSdkDto Ticket { get; set; } = default!;

        public List<MessagingService.Sdk.Dtos.MessageDto> Messages { get; set; } = [];

        public Dictionary<string, string> UserNames { get; set; } = [];
    }
}
