using EmailService.Domain.Models;

namespace EmailService.Sdk.Services
{
    public interface IEmailSdkService
    {

        Task NotifyTicketCreatedAsync(Guid customerId,string ticketTitle, int ticketId);

        Task NotifyTicketStatusChangedAsync(
            Guid customerId,
            string ticketTitle,
            int ticketId,
            string oldStatus,
            string newStatus,
            string agentName,
            string agentRole,
            string agentNote);

        Task NotifyTicketRepliedAsync(
            Guid customerId,
            string ticketTitle,
            int ticketId,
            string agentName,
            string agentRole,
            string replyContent);

    }
}
