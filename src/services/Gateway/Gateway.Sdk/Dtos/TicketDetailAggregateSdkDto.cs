using System.Text.Json.Serialization;
using MessagingService.Sdk.Dtos;
using TicketService.Sdk.Dtos;

namespace Gateway.Sdk.Dtos
{
    public class TicketDetailAggregateSdkDto
    {
        [JsonPropertyName("ticket")]
        public TicketSdkDto Ticket { get; set; } = default!;

        [JsonPropertyName("messages")]
        public List<MessageDto> Messages { get; set; } = new();

        [JsonPropertyName("userNames")]
        public Dictionary<string, string> UserNames { get; set; } = new();
    }
}
