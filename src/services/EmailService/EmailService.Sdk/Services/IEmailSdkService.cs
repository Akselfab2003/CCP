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
        //Task NotifySupportCustomerReplyAsync(
        //    Guid customerId,
        //    string agentEmail,
        //    string agentName,
        //    int ticketId,
        //    string ticketTitle,
        //    string ticketStatus,
        //    string ticketStatusLabel,
        //    string replyContent);

        //Task NotifySupportNewTicketAsync(
        //    Guid customerId,
        //    string supportTeamEmail,
        //    string ticketTitle,
        //    int ticketId,
        //    string ticketBody);

    }
}
