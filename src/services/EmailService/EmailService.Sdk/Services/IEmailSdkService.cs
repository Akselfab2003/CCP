using EmailService.Domain.Models;

namespace EmailService.Sdk.Services
{
    public interface IEmailSdkService
    {

        Task NotifyTicketCreatedAsync(Guid customerId, int ticketId);

        Task NotifyTicketStatusChangedAsync(
            Guid customerId,
            int ticketId,
            string oldStatus,
            string newStatus,
            string agentName,
            string agentRole,
            string agentNote);

        Task NotifyTicketRepliedAsync(
            Guid customerId,
            int ticketId,
            string agentName,
            string agentRole,
            string replyContent);

    }
}
