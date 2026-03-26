using System;
using CCP.Sdk.utils.Abstractions;

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
            try
            {
                // TODO: Implement API call once Models and API endpoints are defined
                // await _client.Client.Api.EmailSender.SendTicketCreated.PostAsync(...)
                await Task.CompletedTask;
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
                // TODO: Implement API call once Models and API endpoints are defined
                // await _client.Client.Api.EmailSender.SendTicketReply.PostAsync(...)
                await Task.CompletedTask;
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
                // TODO: Implement API call once Models and API endpoints are defined
                // await _client.Client.Api.EmailSender.SendTicketStatus.PostAsync(...)
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to send ticket status email.", ex);
            }
        }

        private static string GenerateSenderAddress(string prefix, string organizationName)
        {
            var sanitizedOrg = organizationName.Replace(" ", "").ToLower();
            return $"{prefix}@{sanitizedOrg}.com";
        }

        private static string ExtractFirstName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return "support";

            var parts = fullName.Split(' ');
            return parts.Length > 0 ? parts[0].ToLower() : "support";
        }
    }
}
