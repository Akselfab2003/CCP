using MessagingService.Api.Hubs;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.MessagingService.Tests;

/// <summary>
/// Records AddToGroupAsync / RemoveFromGroupAsync calls so we can assert on them.
/// </summary>
public class FakeGroupManager : IGroupManager
{
    public List<(string ConnectionId, string GroupName)> Added { get; } = new();
    public List<(string ConnectionId, string GroupName)> Removed { get; } = new();

    public Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
    {
        Added.Add((connectionId, groupName));
        return Task.CompletedTask;
    }

    public Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
    {
        Removed.Add((connectionId, groupName));
        return Task.CompletedTask;
    }
}

/// <summary>
/// Minimal fake of HubCallerContext that just exposes a ConnectionId.
/// </summary>
public class FakeHubCallerContext : HubCallerContext
{
    private readonly string _connectionId;

    public FakeHubCallerContext(string connectionId)
    {
        _connectionId = connectionId;
    }

    public override string ConnectionId => _connectionId;
    public override string? UserIdentifier => null;
    public override System.Security.Claims.ClaimsPrincipal User => new();
    public override IDictionary<object, object?> Items => new Dictionary<object, object?>();
    public override IFeatureCollection Features => null!;
    public override CancellationToken ConnectionAborted => CancellationToken.None;

    public override void Abort() { }
}

public class ChatHubTests
{
    private static ChatHub CreateHub(string connectionId, out FakeGroupManager groups)
    {
        groups = new FakeGroupManager();
        var hub = new ChatHub();

        // Hub.Groups and Hub.Context are settable via the base Hub class properties
        hub.Groups = groups;
        hub.Context = new FakeHubCallerContext(connectionId);

        return hub;
    }

    [Fact]
    public async Task JoinTicketGroup_AddsConnectionToCorrectGroup()
    {
        // Arrange
        const string connectionId = "conn-abc-123";
        const int ticketId = 42;
        var hub = CreateHub(connectionId, out var groups);

        // Act
        await hub.JoinTicketGroup(ticketId);

        // Assert
        var added = Assert.Single(groups.Added);
        Assert.Equal(connectionId, added.ConnectionId);
        Assert.Equal("ticket-42", added.GroupName);
        Assert.Empty(groups.Removed);
    }

    [Fact]
    public async Task LeaveTicketGroup_RemovesConnectionFromCorrectGroup()
    {
        // Arrange
        const string connectionId = "conn-xyz-789";
        const int ticketId = 99;
        var hub = CreateHub(connectionId, out var groups);

        // Act
        await hub.LeaveTicketGroup(ticketId);

        // Assert
        var removed = Assert.Single(groups.Removed);
        Assert.Equal(connectionId, removed.ConnectionId);
        Assert.Equal("ticket-99", removed.GroupName);
        Assert.Empty(groups.Added);
    }

    [Fact]
    public async Task JoinTicketGroup_WithDifferentTicketIds_CreatesDistinctGroups()
    {
        // Arrange
        const string connectionId = "conn-multi";
        var hub = CreateHub(connectionId, out var groups);

        // Act
        await hub.JoinTicketGroup(1);
        await hub.JoinTicketGroup(200);
        await hub.JoinTicketGroup(9999);

        // Assert
        Assert.Equal(3, groups.Added.Count);
        Assert.Equal("ticket-1", groups.Added[0].GroupName);
        Assert.Equal("ticket-200", groups.Added[1].GroupName);
        Assert.Equal("ticket-9999", groups.Added[2].GroupName);
        Assert.All(groups.Added, entry => Assert.Equal(connectionId, entry.ConnectionId));
    }

    [Fact]
    public async Task JoinAndLeave_SameTicket_RecordsBothOperations()
    {
        // Arrange
        const string connectionId = "conn-join-leave";
        const int ticketId = 55;
        var hub = CreateHub(connectionId, out var groups);

        // Act
        await hub.JoinTicketGroup(ticketId);
        await hub.LeaveTicketGroup(ticketId);

        // Assert
        var added = Assert.Single(groups.Added);
        Assert.Equal("ticket-55", added.GroupName);

        var removed = Assert.Single(groups.Removed);
        Assert.Equal("ticket-55", removed.GroupName);
    }
}
