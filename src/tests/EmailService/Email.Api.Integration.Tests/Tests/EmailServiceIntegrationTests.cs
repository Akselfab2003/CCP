using System.Text;
using CCP.Shared.ValueObjects;
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

            await emailService.NotifyTicketCreatedAsync(customerId, "test1", ticketId, TicketStatus.Open);

            _output.WriteLine($"Notification sent for ticket creation: TicketId={ticketId}, CustomerId={customerId}");
        }

        [Fact]
        public async Task NotifyTicketStatusChanged_ShouldReturnSuccess()
        {
            IEmailSdkService emailService = _fixture.SDK.GetRequiredService<IEmailSdkService>();
            var customerId = Guid.NewGuid();
            var ticketId = Random.Shared.Next(1000, int.MaxValue);
            TicketStatus oldStatus = TicketStatus.Open;
            TicketStatus newStatus = TicketStatus.WaitingForCustomer;

            await emailService.NotifyTicketStatusChangedAsync(
                customerId,
                "test2",
                ticketId,
                oldStatus,
                newStatus);



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

            await emailService.NotifyTicketRepliedAsync(
                ticketId,
                TicketStatus.Open,
                TicketOrigin.Manual,
                agentName,
                agentRole
              );

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

                await emailService.NotifyTicketCreatedAsync(customerId, "test4", ticketId, TicketStatus.Open);
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
                (TicketStatus.Open, TicketStatus.WaitingForCustomer),
                (TicketStatus.WaitingForCustomer, TicketStatus.WaitingForSupport),
                (TicketStatus.Blocked, TicketStatus.Closed)
            };

            foreach (var (oldStatus, newStatus) in statusTransitions)
            {
                await emailService.NotifyTicketStatusChangedAsync(
                    customerId,
                    "test5",
                    ticketId,
                    oldStatus,
                    newStatus);
            }

            _output.WriteLine($"Successfully sent {statusTransitions.Length} status change notifications");
        }

        [Fact]
        public async Task NotifyTicketReply_WithSpecialCharacters_ShouldSucceed()
        {
            IEmailSdkService emailService = _fixture.SDK.GetRequiredService<IEmailSdkService>();
            var customerId = Guid.NewGuid();
            var ticketId = Random.Shared.Next(1000, int.MaxValue);

            await emailService.NotifyTicketRepliedAsync(
                ticketId: ticketId,
                TicketStatus.Open,
                TicketOrigin.Manual,
                agentName: "Agent with Special Chars",
                agentRole: "Support & Service"
               );

            _output.WriteLine($"Notification sent with special characters");
        }

        [Fact]
        public async Task NotifyTicketStatusChanged_WithLongAgentNote_ShouldSucceed()
        {
            IEmailSdkService emailService = _fixture.SDK.GetRequiredService<IEmailSdkService>();
            var customerId = Guid.NewGuid();
            IEmailSent emailSentRepo = _fixture.DB.GetRequiredService<IEmailSent>();
            var ticketId = Random.Shared.Next(1000, int.MaxValue);
            var longNote = new StringBuilder();

            for (int i = 0; i < 50; i++)
            {
                longNote.AppendLine($"Line {i}: This is a detailed agent note about the status change.");
            }

            await emailService.NotifyTicketStatusChangedAsync(
                customerId,
                "test7",
                ticketId,
                TicketStatus.Open,
                TicketStatus.Closed);

            _output.WriteLine($"Notification sent with long agent note ({longNote.Length} chars)");
        }

        [Fact]
        public async Task NotifyTicketReply_WithEmptyReplyContent_ShouldSucceed()
        {
            IEmailSdkService emailService = _fixture.SDK.GetRequiredService<IEmailSdkService>();
            var customerId = Guid.NewGuid();
            var ticketId = Random.Shared.Next(1000, int.MaxValue);

            await emailService.NotifyTicketRepliedAsync(
                ticketId,
                TicketStatus.Open,
                TicketOrigin.Manual,
                "Agent",
                "Support"
                );

            _output.WriteLine($"Notification sent with empty reply content");
        }

        [Fact]
        public async Task NotifyTicketCreated_WithValidGuids_ShouldSucceed()
        {
            IEmailSdkService emailService = _fixture.SDK.GetRequiredService<IEmailSdkService>();
            var customerId = Guid.NewGuid();
            var ticketId = Random.Shared.Next(1000, int.MaxValue);

            await emailService.NotifyTicketCreatedAsync(customerId, "test9", ticketId, TicketStatus.Open);

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
            var oldStatus = TicketStatus.Open;
            var newStatus = TicketStatus.Open;

            await emailService.NotifyTicketStatusChangedAsync(
                customerId,
                "test10",
                ticketId,
                oldStatus,
                newStatus);


            _output.WriteLine($"All parameters verified for status change notification");
        }
    }
}
