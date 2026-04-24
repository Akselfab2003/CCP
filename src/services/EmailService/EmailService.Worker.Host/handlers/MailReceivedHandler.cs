using CCP.Shared.Events;

namespace EmailService.Worker.Host.handlers
{
    public class MailReceivedHandler
    {
        private readonly ILogger<MailReceivedHandler> _logger;

        public MailReceivedHandler(ILogger<MailReceivedHandler> logger) => _logger = logger;

        public void Handle(mail_received mail_Received)
        {
            _logger.LogInformation($"Handled mail_received event: Subject: {mail_Received.Subject}, From: {mail_Received.MailFrom}, To: {mail_Received.MailTo}, MessageId: {mail_Received.MessageId}");
        }
    }
}
