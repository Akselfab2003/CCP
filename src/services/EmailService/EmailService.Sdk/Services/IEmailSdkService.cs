
using CCP.Shared.ValueObjects;
using EmailService.Domain.Models;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace EmailService.Sdk.Services
{
    public interface IEmailSdkService
    {
        Task NotifySupportCustomerReplyAsync(Guid customerId, string agentEmail, string agentName, int ticketId, string ticketTitle, TicketStatus ticketStatus, string replyContent,string orgName);
        Task NotifyTicketCreatedAsync(Guid customerId, string ticketTitle, int ticketId, TicketStatus status, string orgName);
        Task NotifyTicketStatusChangedAsync(Guid customerId, string ticketTitle, int ticketId, TicketStatus oldStatus, TicketStatus newStatus, string orgName);
        Task NotifyTicketRepliedAsync(int ticketId, TicketStatus status, TicketOrigin origin, string agentName, string agentRole,string orgName);
        Task CreateTenantEmailAsync(string DefaultSenderEmail);
        Task UpdateTenantEmailAsync(string DefaultSenderEmail);
        Task<TenantEmailConfiguration?> GetTenantEmailAsync();

    }
}
