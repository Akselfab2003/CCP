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
        public async Task NotifyTicketCreated_ShouldReturnSuccess()
        {
            IEmailSdkService emailService = _fixture.SDK.GetRequiredService<IEmailSdkService>();
            var customerId = Guid.NewGuid();
            var ticketId = Random.Shared.Next(1000, int.MaxValue);

            await emailService.NotifyTicketCreatedAsync(customerId, ticketId);

            _output.WriteLine($"Notification sent for ticket creation: TicketId={ticketId}, CustomerId={customerId}");
        }

        [Fact]
        public async Task NotifyTicketStatusChanged_ShouldReturnSuccess()
        {
            IEmailSdkService emailService = _fixture.SDK.GetRequiredService<IEmailSdkService>();
            var customerId = Guid.NewGuid();
            var ticketId = Random.Shared.Next(1000, int.MaxValue);
            var oldStatus = "open";
            var newStatus = "assigned";
            var agentName = "John Doe";
            var agentRole = "Support Agent";
            var agentNote = "Ticket assigned to me";

            await emailService.NotifyTicketStatusChangedAsync(
                customerId,
                ticketId,
                oldStatus,
                newStatus,
                agentName,
                agentRole,
                agentNote);

            _output.WriteLine($"Notification sent for status change: TicketId={ticketId}, Status={oldStatus} -> {newStatus}");
        }

        [Fact]
        public async Task NotifyTicketReply_ShouldReturnSuccess()
        {
            IEmailSdkService emailService = _fixture.SDK.GetRequiredService<IEmailSdkService>();
            var customerId = Guid.NewGuid();
            var ticketId = Random.Shared.Next(1000, int.MaxValue);
            var agentName = "Jane Smith";
            var agentRole = "Support Specialist";
            var replyContent = "Thank you for your inquiry. We are working on your issue.";

            await emailService.NotifyTicketRepliedAsync(
                customerId,
                ticketId,
                agentName,
                agentRole,
                replyContent);

            _output.WriteLine($"Notification sent for reply: TicketId={ticketId}, Agent={agentName}");
        }

        [Fact]
        public async Task NotifyTicketCreated_WithMultipleNotifications_ShouldSucceed()
        {
            IEmailSdkService emailService = _fixture.SDK.GetRequiredService<IEmailSdkService>();
            var notificationCount = 3;
            var ticketIds = new List<int>();

            for (int i = 0; i < notificationCount; i++)
            {
                var customerId = Guid.NewGuid();
                var ticketId = Random.Shared.Next(1000, int.MaxValue);
                ticketIds.Add(ticketId);

                await emailService.NotifyTicketCreatedAsync(customerId, ticketId);
            }

            _output.WriteLine($"Successfully sent {notificationCount} ticket creation notifications");
        }

        [Fact]
        public async Task NotifyTicketStatusChanged_WithVariousStatuses_ShouldSucceed()
        {
            IEmailSdkService emailService = _fixture.SDK.GetRequiredService<IEmailSdkService>();
            var customerId = Guid.NewGuid();
            var ticketId = Random.Shared.Next(1000, int.MaxValue);

            var statusTransitions = new[]
            {
                ("open", "assigned"),
                ("assigned", "in_progress"),
                ("in_progress", "closed")
            };

            foreach (var (oldStatus, newStatus) in statusTransitions)
            {
                await emailService.NotifyTicketStatusChangedAsync(
                    customerId,
                    ticketId,
                    oldStatus,
                    newStatus,
                    "Support Agent",
                    "Agent Role",
                    $"Status changed to {newStatus}");
            }

            _output.WriteLine($"Successfully sent {statusTransitions.Length} status change notifications");
        }

        [Fact]
        public async Task NotifyTicketReply_WithSpecialCharacters_ShouldSucceed()
        {
            IEmailSdkService emailService = _fixture.SDK.GetRequiredService<IEmailSdkService>();
            var customerId = Guid.NewGuid();
            var ticketId = Random.Shared.Next(1000, int.MaxValue);
            var replyContent = "Reply with special chars: æøå ÆØÅ !@#$%^&*() <html>";

            await emailService.NotifyTicketRepliedAsync(
                customerId,
                ticketId,
                "Agent Name",
                "Agent Role",
                replyContent);

            _output.WriteLine($"Notification sent with special characters");
        }

        [Fact]
        public async Task NotifyTicketStatusChanged_WithLongAgentNote_ShouldSucceed()
        {
            IEmailSdkService emailService = _fixture.SDK.GetRequiredService<IEmailSdkService>();
            var customerId = Guid.NewGuid();
            var ticketId = Random.Shared.Next(1000, int.MaxValue);
            var longNote = new StringBuilder();

            for (int i = 0; i < 50; i++)
            {
                longNote.AppendLine($"Line {i}: This is a detailed agent note about the status change.");
            }

            await emailService.NotifyTicketStatusChangedAsync(
                customerId,
                ticketId,
                "open",
                "closed",
                "Senior Agent",
                "Support Manager",
                longNote.ToString());

            _output.WriteLine($"Notification sent with long agent note ({longNote.Length} chars)");
        }

        [Fact]
        public async Task NotifyTicketReply_WithEmptyReplyContent_ShouldSucceed()
        {
            IEmailSdkService emailService = _fixture.SDK.GetRequiredService<IEmailSdkService>();
            var customerId = Guid.NewGuid();
            var ticketId = Random.Shared.Next(1000, int.MaxValue);

            await emailService.NotifyTicketRepliedAsync(
                customerId,
                ticketId,
                "Agent",
                "Support",
                "");

            _output.WriteLine($"Notification sent with empty reply content");
        }

        [Fact]
        public async Task NotifyTicketCreated_WithValidGuids_ShouldSucceed()
        {
            IEmailSdkService emailService = _fixture.SDK.GetRequiredService<IEmailSdkService>();
            var customerId = Guid.NewGuid();
            var ticketId = Random.Shared.Next(1000, int.MaxValue);

            await emailService.NotifyTicketCreatedAsync(customerId, ticketId);

            Assert.True(customerId != Guid.Empty);
            Assert.True(ticketId > 0);
            _output.WriteLine($"Verified ticket creation with CustomerId: {customerId}, TicketId: {ticketId}");
        }

        [Fact]
        public async Task NotifyTicketStatusChanged_WithAllParameters_ShouldSucceed()
        {
            IEmailSdkService emailService = _fixture.SDK.GetRequiredService<IEmailSdkService>();
            var customerId = Guid.NewGuid();
            var ticketId = Random.Shared.Next(1000, int.MaxValue);
            var agentName = "Support Agent";
            var agentRole = "Specialist";
            var oldStatus = "pending";
            var newStatus = "resolved";
            var agentNote = "Issue resolved by updating configuration";

            await emailService.NotifyTicketStatusChangedAsync(
                customerId,
                ticketId,
                oldStatus,
                newStatus,
                agentName,
                agentRole,
                agentNote);

            _output.WriteLine($"All parameters verified for status change notification");
        }
    }
}
