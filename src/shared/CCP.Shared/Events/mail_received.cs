using CCP.Shared.Events.Interfaces;

namespace CCP.Shared.Events
{
    public class mail_received : IMailBoxMessage
    {
        public long uid { get; set; }
        public string @event { get; set; } = null!;
        public string mailbox { get; set; } = null!;
        public string hostname { get; set; } = null!;
        public string? user { get; set; }
    }
}
