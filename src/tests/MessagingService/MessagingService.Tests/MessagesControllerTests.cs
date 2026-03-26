using MessagingService.Api.Controllers;
using MessagingService.Api.Hubs;
using MessagingService.Domain.Contracts;
using MessagingService.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.MessagingService.Tests;

public class FakeMessageService : IMessageService
{
    public Func<CreateMessageRequest, Task<MessageServiceResult>>? OnCreate { get; set; }
    public Func<int, int, int?, Task<PagedMessagesResponse>>? OnGetByTicket { get; set; }
    public Func<int, Task<MessageResponse?>>? OnGetById { get; set; }
    public Func<int, UpdateMessageRequest, Task<MessageServiceResult>>? OnUpdate { get; set; }
    public Func<int, Task<(bool Deleted, int? TicketId)>>? OnDelete { get; set; }

    public Task<MessageServiceResult> CreateMessageAsync(CreateMessageRequest request, CancellationToken cancellationToken = default)
        => OnCreate?.Invoke(request) ?? Task.FromResult(MessageServiceResult.Failed("Not configured"));

    public Task<PagedMessagesResponse> GetMessagesByTicketIdAsync(int ticketId, int limit, int? beforeMessageId, CancellationToken cancellationToken = default)
        => OnGetByTicket?.Invoke(ticketId, limit, beforeMessageId) ?? Task.FromResult(new PagedMessagesResponse());

    public Task<MessageResponse?> GetMessageByIdAsync(int messageId, CancellationToken cancellationToken = default)
        => OnGetById?.Invoke(messageId) ?? Task.FromResult<MessageResponse?>(null);

    public Task<MessageServiceResult> UpdateMessageAsync(int messageId, UpdateMessageRequest request, CancellationToken cancellationToken = default)
        => OnUpdate?.Invoke(messageId, request) ?? Task.FromResult(MessageServiceResult.Failed("Not configured"));

    public Task<(bool Deleted, int? TicketId)> SoftDeleteMessageAsync(int messageId, CancellationToken cancellationToken = default)
        => OnDelete?.Invoke(messageId) ?? Task.FromResult((false, (int?)null));
}

public class FakeHubClients : IHubClients
{
    public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => new FakeClientProxy();
    public IClientProxy Client(string connectionId) => new FakeClientProxy();
    public IClientProxy Clients(IReadOnlyList<string> connectionIds) => new FakeClientProxy();
    public IClientProxy Group(string groupName) => new FakeClientProxy();
    public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) => new FakeClientProxy();
    public IClientProxy Groups(IReadOnlyList<string> groupNames) => new FakeClientProxy();
    public IClientProxy User(string userId) => new FakeClientProxy();
    public IClientProxy Users(IReadOnlyList<string> userIds) => new FakeClientProxy();
    public IClientProxy All => new FakeClientProxy();
}

public class FakeClientProxy : IClientProxy
{
    public Task SendCoreAsync(string method, object?[] args, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

public class FakeHubContext : IHubContext<ChatHub>
{
    public IHubClients Clients => new FakeHubClients();
    public IGroupManager Groups => throw new NotImplementedException();
}

public class MessagesControllerTests
{
    [Fact]
    public async Task CreateMessage_WhenServiceSucceeds_ReturnsCreatedAtAction()
    {
        var fakeService = new FakeMessageService
        {
            OnCreate = _ => Task.FromResult(MessageServiceResult.Succeeded(new MessageResponse
            {
                Id = 123,
                TicketId = 1,
                UserId = Guid.NewGuid(),
                OrganizationId = Guid.NewGuid(),
                Content = "Created"
            }))
        };

        var controller = new MessagesController(fakeService, new FakeHubContext());

        var result = await controller.CreateMessage(new CreateMessageRequest
        {
            TicketId = 1,
            UserId = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Content = "Created"
        }, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var body = Assert.IsType<MessageResponse>(created.Value);

        Assert.Equal(123, body.Id);
        Assert.Equal(nameof(MessagesController.GetMessageById), created.ActionName);
    }

    [Fact]
    public async Task GetMessageById_WhenMissing_ReturnsNotFound()
    {
        var fakeService = new FakeMessageService
        {
            OnGetById = _ => Task.FromResult<MessageResponse?>(null)
        };

        var controller = new MessagesController(fakeService, new FakeHubContext());

        var result = await controller.GetMessageById(999, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task DeleteMessage_WhenServiceReturnsFalse_ReturnsNotFound()
    {
        var fakeService = new FakeMessageService
        {
            OnDelete = _ => Task.FromResult((false, (int?)null))
        };

        var controller = new MessagesController(fakeService, new FakeHubContext());

        var result = await controller.DeleteMessage(999, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetMessagesByTicketId_WithInvalidTicketId_ReturnsBadRequest()
    {
        var fakeService = new FakeMessageService();
        var controller = new MessagesController(fakeService, new FakeHubContext());

        var result = await controller.GetMessagesByTicketId(0, 50, null, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("TicketId must be greater than 0.", badRequest.Value);
    }
}
