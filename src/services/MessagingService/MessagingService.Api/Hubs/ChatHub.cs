using Microsoft.AspNetCore.SignalR;

namespace MessagingService.Api.Hubs;

public class ChatHub : Hub
{
    // Client calls this when they open a ticket/chat to start receiving real-time messages
    public async Task JoinTicketGroup(int ticketId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"ticket-{ticketId}");
    }

    // Client calls this when they navigate away from a ticket/chat
    public async Task LeaveTicketGroup(int ticketId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"ticket-{ticketId}");
    }
}
