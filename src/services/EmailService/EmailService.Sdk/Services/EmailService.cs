using CCP.Sdk.utils.Abstractions;
using EmailService.Domain.Models;
using EmailService.Sdk.Services;

namespace EmailService.Sdk.Services
{
    public class EmailService : IEmailService
    {
        private readonly IKiotaApiClient<EmailServiceClient> _client;

        public EmailService(IKiotaApiClient<EmailServiceClient> client)
        {
            _client = client;
        }

        public async Task SendTicketCreatedEmailAsync(
            int ticketId,
            string subject,
            string body,
            string recipientEmail,
            string organizationName = "Support",
            string expectedResponseTime = "24 hours",
            string portalUrl = "#")
        {
            var request = new TicketCreatedRequest
            {
                Email = new EmailSent
                {
                    Id = ticketId,
                    OrganizationId = Guid.NewGuid(),
                    Subject = subject,
                    Body = body,
                    SenderAddress = $"support@{organizationName.Replace(" ", "").ToLowerInvariant()}.com",
                    RecipientAddress = recipientEmail,
                    SentAt = DateTime.UtcNow
                },
                RecipientName = recipientEmail,
                OrganizationName = organizationName,
                ExpectedResponseTime = expectedResponseTime,
                PortalUrl = portalUrl
            };

            await _client.Client.Api.EmailSendingService.SendTicketCreated.PostAsync(request);
        }

        public async Task SendTicketReplyEmailAsync(
            int ticketId,
            string subject,
            string body,
            string recipientEmail,
            string recipientName,
            string agentName,
            string agentRole = "Support Agent",
            string organizationName = "Support",
            string ticketStatus = "open",
            string ticketStatusLabel = "Open",
            string replyUrl = "#",
            string portalUrl = "#",
            string viewHistoryUrl = "#",
            string reopenUrl = "#")
        {
            var firstName = string.IsNullOrWhiteSpace(agentName) ? "support" : agentName.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0].ToLowerInvariant();

            var request = new TicketReplyRequest
            {
                Email = new EmailReceived
                {
                    Id = ticketId,
                    OrganizationId = Guid.NewGuid(),
                    Subject = subject,
                    Body = body,
                    SenderAddress = $"{firstName}@{organizationName.Replace(" ", "").ToLowerInvariant()}.com",
                    RecipientAddress = recipientEmail,
                    ReceivedAt = DateTime.UtcNow
                },
                RecipientName = recipientName,
                OrganizationName = organizationName,
                AgentName = agentName,
                AgentRole = agentRole,
                TicketStatus = ticketStatus,
                TicketStatusLabel = ticketStatusLabel,
                ReplyUrl = replyUrl,
                PortalUrl = portalUrl,
                ViewHistoryUrl = viewHistoryUrl,
                ReopenUrl = reopenUrl
            };

            await _client.Client.Api.EmailSendingService.SendTicketReply.PostAsync(request);
        }

        public async Task SendTicketStatusEmailAsync(
            int ticketId,
            string subject,
            string recipientEmail,
            string newStatus,
            string newStatusLabel,
            string oldStatusLabel = "Open",
            string organizationName = "Support",
            string updatedByAgent = "",
            string agentNote = "",
            string portalUrl = "#",
            string reopenUrl = "#")
        {
            var request = new TicketStatusRequest
            {
                Email = new EmailSent
                {
                    Id = ticketId,
                    OrganizationId = Guid.NewGuid(),
                    Subject = subject,
                    Body = string.Empty,
                    SenderAddress = $"support@{organizationName.Replace(" ", "").ToLowerInvariant()}.com",
                    RecipientAddress = recipientEmail,
                    SentAt = DateTime.UtcNow
                },
                RecipientName = recipientEmail,
                OrganizationName = organizationName,
                NewStatus = newStatus,
                NewStatusLabel = newStatusLabel,
                OldStatusLabel = oldStatusLabel,
                UpdatedByAgent = updatedByAgent,
                AgentNote = agentNote,
                PortalUrl = portalUrl,
                ReopenUrl = reopenUrl
            };

            await _client.Client.Api.EmailSendingService.SendTicketStatus.PostAsync(request);
        }
    }
}
