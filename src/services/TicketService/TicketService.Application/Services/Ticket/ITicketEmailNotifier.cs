using System;
using System.Collections.Generic;
using System.Text;

namespace TicketService.Application.Services.Ticket
{
    public interface ITicketEmailNotifier
    {
        Task NotifyTicketCreatedAsync(int ticketId);
        Task NotifyTicketStatusChangedAsync(int ticketId, string oldStatus, string newStatus, string agentName, string agentRole, string agentNote);
        Task NotifyTicketRepliedAsync(int ticketId, string agentName, string agentRole);
    }
}
