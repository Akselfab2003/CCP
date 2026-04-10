using CCP.Shared.Events;

namespace EmailService.Worker.Host.handlers
{
    public class MailReceivedHandler
    {
        private readonly ILogger<MailReceivedHandler> _logger;

        public MailReceivedHandler(ILogger<MailReceivedHandler> logger) => _logger = logger;

        public void Handle(mail_received mail_Received)
        {
            _logger.LogInformation("Received mail event for mailbox: {mailbox}, user: {user}, uid: {uid}", mail_Received.mailbox, mail_Received.user, mail_Received.uid);
        }
    }
}
