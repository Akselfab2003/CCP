using MessagingService.Sdk.Dtos;          // instead of ChatApp.MessagingService.Contracts
using Microsoft.AspNetCore.SignalR.Client;

namespace CCP.UI.Services;

public class ChatHubService : IAsyncDisposable
{
    private readonly HubConnection _hubConnection;
    private int? _currentTicketId;

    // Now using MessageDto — the SDK type the UI already knows about
    public event Action<MessageDto>? OnMessageReceived;
    public event Action<MessageDto>? OnMessageUpdated;
    public event Action<int>? OnMessageDeleted;
    public event Action<int, Guid>? OnTicketAssigned;

    public ChatHubService(IConfiguration configuration)
    {
        var messagingServiceUrl = configuration.GetValue<string>(
            "services:messagingservice-api:http:0")
            ?? throw new InvalidOperationException(
                "MessagingServiceUrl configuration value is required.");

        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{messagingServiceUrl}/chathub")
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<MessageDto>("ReceiveMessage", message =>
        {
            OnMessageReceived?.Invoke(message);
        });

        _hubConnection.On<MessageDto>("MessageUpdated", message =>
        {
            OnMessageUpdated?.Invoke(message);
        });

        _hubConnection.On<int>("MessageDeleted", messageId =>
        {
            OnMessageDeleted?.Invoke(messageId);
        });

        _hubConnection.On<int, Guid>("TicketAssigned", (ticketId, assignedUserId) =>
        {
            OnTicketAssigned?.Invoke(ticketId, assignedUserId);
        });
    }

    public async Task StartAsync()
    {
        if (_hubConnection.State == HubConnectionState.Disconnected)
        {
            await _hubConnection.StartAsync();
        }
    }

    public async Task JoinTicketAsync(int ticketId)
    {
        // Leave previous ticket group if we were in one
        if (_currentTicketId.HasValue)
        {
            await LeaveTicketAsync(_currentTicketId.Value);
        }

        await _hubConnection.InvokeAsync("JoinTicketGroup", ticketId);
        _currentTicketId = ticketId;
    }

    public async Task JoinTicketGroupAsync(int ticketId)
    {
        // Join without leaving the current active ticket — used for background subscriptions
        await _hubConnection.InvokeAsync("JoinTicketGroup", ticketId);
    }

    public async Task LeaveTicketAsync(int ticketId)
    {
        await _hubConnection.InvokeAsync("LeaveTicketGroup", ticketId);

        if (_currentTicketId == ticketId)
        {
            _currentTicketId = null;
        }
    }

    public bool IsConnected =>
        _hubConnection.State == HubConnectionState.Connected;

    public async ValueTask DisposeAsync()
    {
        if (_currentTicketId.HasValue)
        {
            await LeaveTicketAsync(_currentTicketId.Value);
        }

        await _hubConnection.DisposeAsync();
    }
}
