namespace Email.Application.Tests
{
    public class EmailSenderTests
    {
        //private readonly EmailLogic _emailLogic = new(new FakeSmtpClient());

        //[Fact]
        //public async Task SendHtmlEmail_SendsSuccessfully()
        //{
        //    await _emailLogic.SendHtmlEmail(
        //        fromAddress: "from@test.com",
        //        fromName: "Test Sender",
        //        toAddress: "to@test.com",
        //        toName: "Test Recipient",
        //        subject: "Test Subject",
        //        htmlContent: "<h1>Hello</h1>");
        //}

        //[Fact]
        //public async Task SendEmailNotification_SendsSuccessfully()
        //{
        //    await _emailLogic.SendEmailNotification(
        //        userId: Guid.NewGuid(),
        //        toEmail: "to@test.com",
        //        fromEmail: "from@test.com",
        //        toUser: "Test Recipient",
        //        text: "Plain text body",
        //        subject: "Test Notification");
        //}

        //private class FakeSmtpClient : ISmtpClient
        //{
        //    public Task SendAsync(MimeMessage message) => Task.CompletedTask;
        //}
    }
}
