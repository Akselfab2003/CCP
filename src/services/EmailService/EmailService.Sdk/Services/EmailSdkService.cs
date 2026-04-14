using System.Reflection;
using CCP.Sdk.utils.Abstractions;
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
        public async Task NotifyTicketCreatedAsync(Guid customerId, int ticketId)
        {
            var api = _client.Client;

            await api.Api
                .EmailSendingService
                .PostAsync(request =>
                {
                    request.QueryParameters.CustomerId = customerId;
                    request.QueryParameters.TicketId = ticketId;
                });
        }

        public async Task NotifyTicketStatusChangedAsync(
            Guid customerId,
            int ticketId,
            string oldStatus,
            string newStatus,
            string agentName,
            string agentRole,
            string agentNote)
        {
            var api = _client.Client;

            await api.Api
                .EmailSendingService
                .StatusChange
                .PostAsync(request =>
                {
                    request.QueryParameters.CustomerId = customerId;
                    request.QueryParameters.TicketId = ticketId;
                    request.QueryParameters.NewStatus = newStatus;
                    request.QueryParameters.OldStatus = oldStatus;
                });
        }

        public async Task NotifyTicketRepliedAsync(
            Guid customerId,
            int ticketId,
            string agentName,
            string agentRole,
            string replyContent)
        {
            var api = _client.Client;

            await api.Api
                .EmailSendingService
                .Reply
                .PostAsync(request =>
                {
                    request.QueryParameters.CustomerId = customerId;
                    request.QueryParameters.TicketId = ticketId;
                    request.QueryParameters.AgentName = agentName;
                    request.QueryParameters.AgentRole = agentRole;
                    request.QueryParameters.ReplyContent = replyContent;
                });
        }
    }
}
