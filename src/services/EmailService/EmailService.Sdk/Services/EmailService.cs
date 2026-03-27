using System.Reflection;
using CCP.Sdk.utils.Abstractions;
using EmailService.Sdk.Models;

namespace EmailService.Sdk.Services
{
    internal class EmailService : IEmailService
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
            try
            {
                await _client.Client.Api.EmailSendingService.SendTicketCreated.PostAsync(new TicketCreatedRequest()
                {
                    Email = new EmailSent()
                    {
                        Id = ticketId,
                        OrganizationId = Guid.NewGuid(),
                        Subject = subject,
                        Body = body,
                        SenderAddress = $"support@{organizationName.Replace(" ", "").ToLower()}.com",
                        RecipientAddress = recipientEmail,
                        SentAt = DateTime.UtcNow,
                    },
                    ExpectedResponseTime = expectedResponseTime,
                    OrganizationName = organizationName,
                    PortalUrl = portalUrl,
                });
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to send ticket created email.", ex);
            }
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
            try
            {
                await _client.Client.Api.EmailSendingService.SendTicketReply.PostAsync(new TicketReplyRequest()
                {
                    Email = new EmailReceived()
                    {
                        Id = ticketId,
                        OrganizationId = Guid.NewGuid(),
                        Subject = subject,
                        Body = body,
                        SenderAddress = $"{agentName.Split(' ')[0].ToLower()}@{organizationName.Replace(" ", "").ToLower()}.com",
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
                });
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to send ticket reply email.", ex);
            }
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
            try
            {
                await _client.Client.Api.EmailSendingService.SendTicketStatus.PostAsync(new TicketStatusRequest()
                {
                    Email = new EmailSent()
                    {
                        Id = ticketId,
                        OrganizationId = Guid.NewGuid(),
                        Subject = subject,
                        Body = "",
                        SenderAddress = $"support@{organizationName.Replace(" ", "").ToLower()}.com",
                        RecipientAddress = recipientEmail,
                        SentAt = DateTime.UtcNow,
                    },
                    OrganizationName = organizationName,
                    NewStatus = newStatus,
                    NewStatusLabel = newStatusLabel,
                    OldStatusLabel = oldStatusLabel,
                    UpdatedByAgent = updatedByAgent,
                    AgentNote = agentNote,
                    PortalUrl = portalUrl,
                    ReopenUrl = reopenUrl
                });
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to send ticket status email.", ex);
            }
        }
    }
}
