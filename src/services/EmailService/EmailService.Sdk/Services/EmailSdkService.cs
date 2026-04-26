using CCP.Sdk.utils.Abstractions;
using CCP.Shared.ValueObjects;
using EmailService.Sdk.Models;

namespace EmailService.Sdk.Services
{
    internal class EmailSdkService : IEmailSdkService
    {
        private readonly IKiotaApiClient<EmailServiceClient> _client;
        public EmailSdkService(IKiotaApiClient<EmailServiceClient> client)
        {
            _client = client;
        }
        public async Task NotifyTicketCreatedAsync(Guid customerId, string ticketTitle, int ticketId, TicketStatus status)
        {
            var api = _client.Client;

            await api.Api
                .EmailSendingService
                .PostAsync(request =>
                {
                    request.QueryParameters.CustomerId = customerId;
                    request.QueryParameters.TicketTitle = ticketTitle;
                    request.QueryParameters.TicketId = ticketId;
                    request.QueryParameters.TicketStatus = status.ToString();
                });
        }

        public async Task NotifyTicketStatusChangedAsync(
            Guid customerId,
            string ticketTitle,
            int ticketId,
            TicketStatus oldStatus,
            TicketStatus newStatus)

        {
            var api = _client.Client;

            await api.Api
                .EmailSendingService
                .StatusChange
                .PostAsync(request =>
                {
                    request.QueryParameters.CustomerId = customerId;
                    request.QueryParameters.TicketTitle = ticketTitle;
                    request.QueryParameters.TicketId = ticketId;
                    request.QueryParameters.OldStatus = oldStatus.ToString();
                    request.QueryParameters.NewStatus = newStatus.ToString();
                });
        }

        public async Task NotifyTicketRepliedAsync(
            Guid customerId,
            string ticketTitle,
            int ticketId,
            string agentName,
            string agentRole)
        {
            var api = _client.Client;

            await api.Api
                .EmailSendingService
                .Reply
                .PostAsync(request =>
                {
                    request.QueryParameters.CustomerId = customerId;
                    request.QueryParameters.TicketTitle = ticketTitle;
                    request.QueryParameters.TicketId = ticketId;
                    request.QueryParameters.AgentName = agentName;
                    request.QueryParameters.AgentRole = agentRole;
                });
        }

        public async Task NotifySupportCustomerReplyAsync(
            Guid customerId,
            string agentEmail,
            string agentName,
            int ticketId,
            string ticketTitle,
            TicketStatus ticketStatus,
            string replyContent)
        {
            var api = _client.Client;

            await api.Api
                .EmailSendingService
                .Support
                .CustomerReplied
                .PostAsync(request =>
                {
                    request.QueryParameters.CustomerId = customerId;
                    request.QueryParameters.AgentEmail = agentEmail;
                    request.QueryParameters.AgentName = agentName;
                    request.QueryParameters.TicketId = ticketId;
                    request.QueryParameters.TicketTitle = ticketTitle;
                    request.QueryParameters.TicketStatus = ticketStatus.ToString();
                    request.QueryParameters.ReplyContent = replyContent;
                });
        }
        public async Task CreateTenantEmailAsync(string DefaultSenderEmail)
        {
            var api = _client.Client;

            await api.Api
                .TenantEmailConfiguration
                .Create
                .PostAsync(request=>
                {
                    request.QueryParameters.DefaultSenderEmail = DefaultSenderEmail;
                });
        }
    }
}
