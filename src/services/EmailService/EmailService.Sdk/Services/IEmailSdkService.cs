
using CCP.Shared.ValueObjects;

namespace EmailService.Sdk.Services
{
    public interface IEmailSdkService
    {
        Task NotifySupportCustomerReplyAsync(Guid customerId, string agentEmail, string agentName, int ticketId, string ticketTitle, TicketStatus ticketStatus, string replyContent);
        Task NotifyTicketCreatedAsync(Guid customerId, string ticketTitle, int ticketId);
        Task NotifyTicketRepliedAsync(Guid customerId, string ticketTitle, int ticketId, string agentName, string agentRole);
        Task NotifyTicketStatusChangedAsync(Guid customerId, string ticketTitle, int ticketId, TicketStatus oldStatus, TicketStatus newStatus);
    }
}
