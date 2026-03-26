using System.Net;
using System.Net.Http.Json;
using MessagingService.Domain.Contracts;

namespace ChatApp.MessagingService.Tests;

[Trait("Category", "Integration")]
public class MessageApiIntegrationTests : IClassFixture<MessagingServiceApiFactory>
{
    private readonly HttpClient _client;

    public MessageApiIntegrationTests(MessagingServiceApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostMessage_WithValidRequest_ReturnsCreated()
    {
        var organizationId = Guid.NewGuid();

        var request = new CreateMessageRequest
        {
            TicketId = 1,
            UserId = Guid.NewGuid(),
            OrganizationId = organizationId,
            Content = "Integration test message",
            Embedding = null
        };

        var response = await _client.PostAsJsonAsync("/api/messages", request, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<MessageResponse>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(body);
        Assert.Equal(1, body.TicketId);
        Assert.Equal(organizationId, body.OrganizationId);
        Assert.Equal("Integration test message", body.Content);
        Assert.False(body.IsDeleted);
    }

    [Fact]
    public async Task GetMessagesByTicketId_AfterCreatingMessage_ReturnsMessage()
    {
        var organizationId = Guid.NewGuid();

        var createRequest = new CreateMessageRequest
        {
            TicketId = 5,
            UserId = Guid.NewGuid(),
            OrganizationId = organizationId,
            Content = "Hello API test",
            Embedding = null
        };

        var createResponse = await _client.PostAsJsonAsync("/api/messages", createRequest, cancellationToken: TestContext.Current.CancellationToken);
        createResponse.EnsureSuccessStatusCode();

        var response = await _client.GetAsync("/api/messages/ticket/5?limit=50", cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PagedMessagesResponse>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(body);
        Assert.Single(body.Items);
        Assert.Equal("Hello API test", body.Items[0].Content);
        Assert.Equal(5, body.Items[0].TicketId);
        Assert.Equal(organizationId, body.Items[0].OrganizationId);
        Assert.False(body.HasMore);
    }

    [Fact]
    public async Task GetMessageById_WithExistingMessage_ReturnsMessage()
    {
        var organizationId = Guid.NewGuid();

        var createRequest = new CreateMessageRequest
        {
            TicketId = 7,
            UserId = Guid.NewGuid(),
            OrganizationId = organizationId,
            Content = "Find me",
            Embedding = null
        };

        var createResponse = await _client.PostAsJsonAsync("/api/messages", createRequest, cancellationToken: TestContext.Current.CancellationToken);
        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content.ReadFromJsonAsync<MessageResponse>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(created);

        var response = await _client.GetAsync($"/api/messages/{created.Id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<MessageResponse>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(body);
        Assert.Equal(created.Id, body.Id);
        Assert.Equal("Find me", body.Content);
        Assert.Equal(7, body.TicketId);
        Assert.Equal(organizationId, body.OrganizationId);
    }

    [Fact]
    public async Task PutMessage_WithValidRequest_UpdatesMessage()
    {
        var createRequest = new CreateMessageRequest
        {
            TicketId = 9,
            UserId = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Content = "Original content",
            Embedding = null
        };

        var createResponse = await _client.PostAsJsonAsync("/api/messages", createRequest, cancellationToken: TestContext.Current.CancellationToken);
        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content.ReadFromJsonAsync<MessageResponse>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(created);

        var updateRequest = new UpdateMessageRequest
        {
            Content = "Updated content",
            Embedding = null
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/messages/{created.Id}", updateRequest, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<MessageResponse>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(updated);
        Assert.Equal("Updated content", updated.Content);
        Assert.True(updated.IsEdited);
        Assert.NotNull(updated.UpdatedAtUtc);
    }

    [Fact]
    public async Task DeleteMessage_WithExistingMessage_ReturnsNoContent_AndMarksAsDeleted()
    {
        var createRequest = new CreateMessageRequest
        {
            TicketId = 11,
            UserId = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Content = "Delete me",
            Embedding = null
        };

        var createResponse = await _client.PostAsJsonAsync("/api/messages", createRequest, cancellationToken: TestContext.Current.CancellationToken);
        createResponse.EnsureSuccessStatusCode();

        var created = await createResponse.Content.ReadFromJsonAsync<MessageResponse>(cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(created);

        var deleteResponse = await _client.DeleteAsync($"/api/messages/{created.Id}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/messages/{created.Id}", TestContext.Current.CancellationToken);
        getResponse.EnsureSuccessStatusCode();

        var deletedMessage = await getResponse.Content.ReadFromJsonAsync<MessageResponse>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(deletedMessage);
        Assert.True(deletedMessage.IsDeleted);
        Assert.Equal("[deleted]", deletedMessage.Content);
    }

    [Fact]
    public async Task PostMessage_WithInvalidEmbeddingLength_ReturnsBadRequest()
    {
        var request = new CreateMessageRequest
        {
            TicketId = 1,
            UserId = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Content = "Bad embedding",
            Embedding = new float[] { 0.1f, 0.2f, 0.3f }
        };

        var response = await _client.PostAsJsonAsync("/api/messages", request, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostMessage_WithNullUserId_ReturnsCreated()
    {
        var request = new CreateMessageRequest
        {
            TicketId = 15,
            UserId = null,
            OrganizationId = Guid.NewGuid(),
            Content = "System generated or anonymous message",
            Embedding = null
        };

        var response = await _client.PostAsJsonAsync("/api/messages", request, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<MessageResponse>(cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(body);
        Assert.Null(body.UserId);
        Assert.Equal(15, body.TicketId);
    }
}
