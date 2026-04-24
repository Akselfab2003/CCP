using CCP.Shared.Events.Interfaces;

namespace CCP.Shared.Events
{
    public class mail_received : IMailBoxMessage
    {
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string MailFrom { get; set; } = string.Empty;
        public string MailTo { get; set; } = string.Empty;
        public string MessageId { get; set; } = string.Empty;
    }
}
