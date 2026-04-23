using MessagingService.Sdk.Dtos;
using TicketService.Sdk.Dtos;

namespace Gateway.Api.Dtos
{
    public class TicketDetailAggregateDto
    {
        public TicketSdkDto Ticket { get; set; } = default!;
        public List<MessageDto> Messages { get; set; } = new();
        public Dictionary<string, string> UserNames { get; set; } = new();
    }
}
