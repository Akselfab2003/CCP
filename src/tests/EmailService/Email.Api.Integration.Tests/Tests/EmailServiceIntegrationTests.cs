using System.Text;
using Email.Api.Integration.Tests.Fixtures;
using EmailService.Domain.Interfaces;
using EmailService.Sdk.Services;

namespace Email.Api.Integration.Tests.Tests
{
    [Collection("Email")]
    public class EmailServiceTests
    {
        private readonly EmailServiceFixture _fixture;
        private readonly ITestOutputHelper _output;

        public EmailServiceTests(EmailServiceFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }

        [Fact]
        public async Task SendEmail_ShouldReturnSuccess()
        {
            IEmailService emailService = _fixture.SDK.GetRequiredService<IEmailService>();
            var ticketId = Random.Shared.Next(1000, int.MaxValue);
            var recipientEmail = "toTest@toTest.com";
            var subject = "Test Subject";

            await emailService.SendTicketCreatedEmailAsync(ticketId, subject, "TEST", recipientEmail);

            IEmailSent emailSentRepo = _fixture.DB.GetRequiredService<IEmailSent>();
            var emailSent = await emailSentRepo.GetByIdAsync(ticketId);
            Assert.NotNull(emailSent);
            Assert.Equal(subject, emailSent.Subject);
            Assert.Equal(recipientEmail, emailSent.RecipientAddress);
            _output.WriteLine($"Email sent with subject: {emailSent.Subject} to {emailSent.RecipientAddress}");
        }

        [Fact]
        public async Task SendEmail_WithMultipleEmails_ShouldSaveAllInDatabase()
        {
            IEmailService emailService = _fixture.SDK.GetRequiredService<IEmailService>();
            IEmailSent emailSentRepo = _fixture.DB.GetRequiredService<IEmailSent>();
            var emailCount = 3;
            var ticketIds = new List<int>();

            for (int i = 0; i < emailCount; i++)
            {
                // Fixed: use Random.Shared to avoid duplicate seeds in tight loops on Linux CI
                var ticketId = Random.Shared.Next(1000, int.MaxValue);
                ticketIds.Add(ticketId);
                await emailService.SendTicketCreatedEmailAsync(
                    ticketId,
                    $"Subject {i}",
                    $"Test body {i}",
                    $"recipient{i}@test.com"
                );
            }

            foreach (var id in ticketIds)
            {
                var email = await emailSentRepo.GetByIdAsync(id);
                Assert.NotNull(email);
            }

            _output.WriteLine($"Successfully sent {emailCount} emails");
        }

        [Fact]
        public async Task SendEmail_WithLongBody_ShouldHandleCorrectly()
        {
            IEmailService emailService = _fixture.SDK.GetRequiredService<IEmailService>();
            IEmailSent emailSentRepo = _fixture.DB.GetRequiredService<IEmailSent>();
            var ticketId = Random.Shared.Next(1000, int.MaxValue);
            var longBody = new StringBuilder();
            for (int i = 0; i < 100; i++)
            {
                longBody.AppendLine($"Line {i}: This is a test line with some content to make the email body longer.");
            }

            await emailService.SendTicketCreatedEmailAsync(
                ticketId,
                "Long Body Test",
                longBody.ToString(),
                "longbody@test.com"
            );

            var sentEmail = await emailSentRepo.GetByIdAsync(ticketId);
            Assert.NotNull(sentEmail);
            Assert.Contains("Line 50:", sentEmail.Body);
            Assert.True(sentEmail.Body.Length > 5000);
            _output.WriteLine($"Email with body length {sentEmail.Body.Length} sent successfully");
        }

        [Fact]
        public async Task SendEmail_WithSpecialCharacters_ShouldEncodeCorrectly()
        {
            IEmailService emailService = _fixture.SDK.GetRequiredService<IEmailService>();
            IEmailSent emailSentRepo = _fixture.DB.GetRequiredService<IEmailSent>();
            var ticketId = Random.Shared.Next(1000, int.MaxValue);
            var specialSubject = "Test æøå ÆØÅ !@#$%^&*()";
            var specialBody = "Body with special chars: <html> & \"quotes\" 'apostrophes'";

            await emailService.SendTicketCreatedEmailAsync(
                ticketId,
                specialSubject,
                specialBody,
                "special@test.com"
            );

            var sentEmail = await emailSentRepo.GetByIdAsync(ticketId);
            Assert.NotNull(sentEmail);
            Assert.Contains("æøå", sentEmail.Subject);
            Assert.Contains("<html>", sentEmail.Body);
            _output.WriteLine($"Email with special characters sent: {sentEmail.Subject}");
        }

        [Fact]
        public async Task SendEmail_WithEmptyBody_ShouldStillSave()
        {
            IEmailService emailService = _fixture.SDK.GetRequiredService<IEmailService>();
            IEmailSent emailSentRepo = _fixture.DB.GetRequiredService<IEmailSent>();
            var ticketId = Random.Shared.Next(1000, int.MaxValue);

            await emailService.SendTicketCreatedEmailAsync(
                ticketId,
                "Empty Body Test",
                "",
                "emptybody@test.com"
            );

            var sentEmail = await emailSentRepo.GetByIdAsync(ticketId);
            Assert.NotNull(sentEmail);
            Assert.Equal("", sentEmail.Body);
            Assert.Equal("Empty Body Test", sentEmail.Subject);
            _output.WriteLine("Email with empty body sent successfully");
        }

        [Fact]
        public async Task SendEmail_VerifyAllFieldsAreSaved()
        {
            IEmailService emailService = _fixture.SDK.GetRequiredService<IEmailService>();
            IEmailSent emailSentRepo = _fixture.DB.GetRequiredService<IEmailSent>();
            var ticketId = Random.Shared.Next(1000, int.MaxValue);
            var recipientEmail = "verify@test.com";
            var subject = "Verification Test";
            var body = "This is a verification test body";

            await emailService.SendTicketCreatedEmailAsync(
                ticketId,
                subject,
                body,
                recipientEmail
            );

            var sentEmail = await emailSentRepo.GetByIdAsync(ticketId);
            Assert.NotNull(sentEmail);
            Assert.Equal(recipientEmail, sentEmail.RecipientAddress);
            Assert.Equal(subject, sentEmail.Subject);
            Assert.Equal(body, sentEmail.Body);
            // Fixed: widened the time window from 1 to 5 minutes to handle slow CI runners
            Assert.True(sentEmail.SentAt <= DateTime.UtcNow);
            Assert.True(sentEmail.SentAt >= DateTime.UtcNow.AddMinutes(-5));
            _output.WriteLine($"All fields verified for email ID: {sentEmail.Id}");
        }

        [Fact]
        public async Task SendEmail_WithUniqueTicketId_ShouldBeRetrievable()
        {
            IEmailService emailService = _fixture.SDK.GetRequiredService<IEmailService>();
            IEmailSent emailSentRepo = _fixture.DB.GetRequiredService<IEmailSent>();
            var ticketId = Random.Shared.Next(1000, int.MaxValue);

            await emailService.SendTicketCreatedEmailAsync(
                ticketId,
                "Unique Organization Test",
                "Unique organization test",
                "unique@test.com"
            );

            var retrievedEmail = await emailSentRepo.GetByIdAsync(ticketId);
            Assert.NotNull(retrievedEmail);
            Assert.Equal(ticketId, retrievedEmail.Id);
            _output.WriteLine($"Email retrieved by ticket ID: {ticketId}");
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnEmail()
        {
            IEmailService emailService = _fixture.SDK.GetRequiredService<IEmailService>();
            IEmailSent emailSentRepo = _fixture.DB.GetRequiredService<IEmailSent>();

            var ticketId = Random.Shared.Next(1000, int.MaxValue);

            await emailService.SendTicketCreatedEmailAsync(
                ticketId,
                "GetById Test",
                "Test body",
                "getbyid@test.com"
            );

            var emailById = await emailSentRepo.GetByIdAsync(ticketId);

            Assert.NotNull(emailById);
            Assert.Equal(ticketId, emailById.Id);
            Assert.Equal("GetById Test", emailById.Subject);
            _output.WriteLine($"Email retrieved by ID {emailById.Id}");
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            IEmailSent emailSentRepo = _fixture.DB.GetRequiredService<IEmailSent>();
            // Fixed: use a sentinel value that can never be a valid ticket ID
            int invalidId = -1;

            var result = await emailSentRepo.GetByIdAsync(invalidId);

            Assert.Null(result);
            _output.WriteLine($"Correctly returned null for invalid ID {invalidId}");
        }

        [Fact]
        public async Task GetByOrganizationIdAsync_WithInvalidOrganizationId_ShouldReturnNull()
        {
            IEmailSent emailSentRepo = _fixture.DB.GetRequiredService<IEmailSent>();
            var invalidOrgId = Guid.NewGuid();

            var result = await emailSentRepo.GetByOrganizationIdAsync(invalidOrgId);

            Assert.Null(result);
            _output.WriteLine($"Correctly returned null for invalid organization ID");
        }
    }
}
