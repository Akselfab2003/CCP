using CCP.Shared.AuthContext;
using MessagingService.Application.Services;
using MessagingService.Domain.Contracts;
using MessagingService.Domain.Entities;
using MessagingService.Domain.Interfaces;
using MessagingService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TicketService.Sdk.Dtos;
using TicketService.Sdk.Services.Ticket;

namespace ChatApp.MessagingService.Tests;

public class TestMessagingDbContext : MessagingDbContext
{
    public TestMessagingDbContext(DbContextOptions<MessagingDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(m => m.Id);

            entity.Property(m => m.TicketId).IsRequired();
            entity.Property(m => m.OrganizationId).IsRequired();
            entity.Property(m => m.UserId).IsRequired(false);

            entity.Property(m => m.Content)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(m => m.CreatedAtUtc).IsRequired();
            entity.Property(m => m.UpdatedAtUtc);
            entity.Property(m => m.DeletedAtUtc);

            entity.Property(m => m.IsEdited).IsRequired();
            entity.Property(m => m.IsDeleted).IsRequired();

            // InMemory DB does not support Vector
            entity.Ignore(m => m.Embedding);
        });
    }
}

public class AllowAllTestMessageAccessValidator : IMessageAccessValidator
{
    public Task<bool> CanSendMessageAsync(
        int ticketId,
        Guid organizationId,
        Guid? userId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }
}

public class MessageServiceTests
{
    private static MessagingDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new TestMessagingDbContext(options);
    }

    private static MessageService CreateService(MessagingDbContext dbContext)
    {
        var validator = new AllowAllTestMessageAccessValidator();
        var ticketService = Substitute.For<ITicketService>();

        ticketService.GetTicket(1).Returns(new TicketSdkDto()
        {
            Id = 1,
            OrganizationId = Guid.NewGuid(),
        });

        var emailService = Substitute.For<EmailService.Sdk.Services.IEmailSdkService>();
        var serviceAccountOverrider = Substitute.For<ServiceAccountOverrider>();
        var logger = Substitute.For<ILogger<MessageService>>();
        return new MessageService(dbContext, validator, serviceAccountOverrider, emailService, ticketService, logger);
    }

    [Fact]
    public async Task CreateMessageAsync_WithValidRequest_ReturnsSuccess()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        var organizationId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var request = new CreateMessageRequest
        {
            TicketId = 1,
            UserId = userId,
            OrganizationId = organizationId,
            Content = "Hello test message",
            Embedding = null
        };

        var result = await service.CreateMessageAsync(request, TestContext.Current.CancellationToken);

        Assert.True(result.Success);
        Assert.Null(result.ErrorMessage);
        Assert.NotNull(result.Message);

        Assert.Equal(1, result.Message.TicketId);
        Assert.Equal(userId, result.Message.UserId);
        Assert.Equal(organizationId, result.Message.OrganizationId);
        Assert.Equal("Hello test message", result.Message.Content);

        var savedMessage = await dbContext.Messages.SingleAsync(cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(1, savedMessage.TicketId);
        Assert.Equal(userId, savedMessage.UserId);
        Assert.Equal(organizationId, savedMessage.OrganizationId);
        Assert.Equal("Hello test message", savedMessage.Content);
    }

    [Fact]
    public async Task CreateMessageAsync_WithNullUserId_ReturnsSuccess()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);


        var request = new CreateMessageRequest
        {
            TicketId = 1,
            UserId = null,
            OrganizationId = Guid.NewGuid(),
            Content = "No explicit user",
            Embedding = null
        };

        var result = await service.CreateMessageAsync(request, TestContext.Current.CancellationToken);

        Assert.True(result.Success);
        Assert.NotNull(result.Message);
        Assert.Null(result.Message.UserId);
    }

    [Fact]
    public async Task CreateMessageAsync_WithInvalidTicketId_ReturnsFailure()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        var request = new CreateMessageRequest
        {
            TicketId = 0,
            UserId = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Content = "Hello test message"
        };

        var result = await service.CreateMessageAsync(request, TestContext.Current.CancellationToken);

        Assert.False(result.Success);
        Assert.Equal("TicketId must be greater than 0.", result.ErrorMessage);
        Assert.Null(result.Message);
        Assert.Empty(dbContext.Messages);
    }

    [Fact]
    public async Task CreateMessageAsync_WithMissingOrganizationId_ReturnsFailure()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        var request = new CreateMessageRequest
        {
            TicketId = 1,
            UserId = Guid.NewGuid(),
            OrganizationId = Guid.Empty,
            Content = "Hello test message"
        };

        var result = await service.CreateMessageAsync(request, TestContext.Current.CancellationToken);

        Assert.False(result.Success);
        Assert.Equal("OrganizationId is required.", result.ErrorMessage);
        Assert.Null(result.Message);
        Assert.Empty(dbContext.Messages);
    }

    [Fact]
    public async Task CreateMessageAsync_WithEmptyContent_ReturnsFailure()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        var request = new CreateMessageRequest
        {
            TicketId = 1,
            UserId = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Content = "   "
        };

        var result = await service.CreateMessageAsync(request, TestContext.Current.CancellationToken);

        Assert.False(result.Success);
        Assert.Equal("Content is required.", result.ErrorMessage);
        Assert.Null(result.Message);
        Assert.Empty(dbContext.Messages);
    }

    [Fact]
    public async Task CreateMessageAsync_WithInvalidEmbeddingLength_ReturnsFailure()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        var request = new CreateMessageRequest
        {
            TicketId = 1,
            UserId = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Content = "Hello test message",
            Embedding = new float[] { 0.1f, 0.2f, 0.3f }
        };

        var result = await service.CreateMessageAsync(request, TestContext.Current.CancellationToken);

        Assert.False(result.Success);
        Assert.Equal("Embedding must contain exactly 1536 values.", result.ErrorMessage);
        Assert.Null(result.Message);
        Assert.Empty(dbContext.Messages);
    }

    [Fact]
    public async Task GetMessagesByTicketIdAsync_ReturnsMessagesInPagedOrder()
    {
        await using var dbContext = CreateDbContext();

        var organizationId = Guid.NewGuid();

        dbContext.Messages.AddRange(
            new Message
            {
                TicketId = 1,
                UserId = Guid.NewGuid(),
                OrganizationId = organizationId,
                Content = "Second",
                CreatedAtUtc = new DateTime(2026, 2, 27, 12, 0, 2, DateTimeKind.Utc)
            },
            new Message
            {
                TicketId = 1,
                UserId = Guid.NewGuid(),
                OrganizationId = organizationId,
                Content = "First",
                CreatedAtUtc = new DateTime(2026, 2, 27, 12, 0, 1, DateTimeKind.Utc)
            },
            new Message
            {
                TicketId = 2,
                UserId = Guid.NewGuid(),
                OrganizationId = organizationId,
                Content = "Other ticket",
                CreatedAtUtc = new DateTime(2026, 2, 27, 12, 0, 0, DateTimeKind.Utc)
            });

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = CreateService(dbContext);

        var result = await service.GetMessagesByTicketIdAsync(1, 50, null, TestContext.Current.CancellationToken);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal("Second", result.Items[0].Content);
        Assert.Equal("First", result.Items[1].Content);
        Assert.False(result.HasMore);
    }

    [Fact]
    public async Task GetMessagesByTicketIdAsync_WithMoreThanLimit_SetsHasMoreTrue()
    {
        await using var dbContext = CreateDbContext();

        var organizationId = Guid.NewGuid();

        dbContext.Messages.AddRange(
            new Message { TicketId = 1, UserId = Guid.NewGuid(), OrganizationId = organizationId, Content = "One", CreatedAtUtc = DateTime.UtcNow },
            new Message { TicketId = 1, UserId = Guid.NewGuid(), OrganizationId = organizationId, Content = "Two", CreatedAtUtc = DateTime.UtcNow },
            new Message { TicketId = 1, UserId = Guid.NewGuid(), OrganizationId = organizationId, Content = "Three", CreatedAtUtc = DateTime.UtcNow });

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = CreateService(dbContext);

        var result = await service.GetMessagesByTicketIdAsync(1, 2, null, TestContext.Current.CancellationToken);

        Assert.Equal(2, result.Items.Count);
        Assert.True(result.HasMore);
    }

    [Fact]
    public async Task GetMessagesByTicketIdAsync_WithBeforeMessageId_ReturnsOnlyOlderMessages()
    {
        await using var dbContext = CreateDbContext();

        var organizationId = Guid.NewGuid();

        var first = new Message
        {
            TicketId = 1,
            UserId = Guid.NewGuid(),
            OrganizationId = organizationId,
            Content = "First",
            CreatedAtUtc = DateTime.UtcNow
        };

        var second = new Message
        {
            TicketId = 1,
            UserId = Guid.NewGuid(),
            OrganizationId = organizationId,
            Content = "Second",
            CreatedAtUtc = DateTime.UtcNow
        };

        var third = new Message
        {
            TicketId = 1,
            UserId = Guid.NewGuid(),
            OrganizationId = organizationId,
            Content = "Third",
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Messages.AddRange(first, second, third);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = CreateService(dbContext);

        var result = await service.GetMessagesByTicketIdAsync(1, 50, third.Id, TestContext.Current.CancellationToken);

        Assert.Equal(2, result.Items.Count);
        Assert.Contains(result.Items, m => m.Id == first.Id);
        Assert.Contains(result.Items, m => m.Id == second.Id);
        Assert.DoesNotContain(result.Items, m => m.Id == third.Id);
    }

    [Fact]
    public async Task GetMessageByIdAsync_WithExistingMessage_ReturnsMessage()
    {
        await using var dbContext = CreateDbContext();

        var message = new Message
        {
            TicketId = 1,
            UserId = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Content = "Lookup me",
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Messages.Add(message);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = CreateService(dbContext);

        var result = await service.GetMessageByIdAsync(message.Id, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(message.Id, result.Id);
        Assert.Equal("Lookup me", result.Content);
        Assert.Equal(1, result.TicketId);
        Assert.Equal(message.OrganizationId, result.OrganizationId);
    }

    [Fact]
    public async Task GetMessageByIdAsync_WithMissingMessage_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();
        var service = CreateService(dbContext);

        var result = await service.GetMessageByIdAsync(999, TestContext.Current.CancellationToken);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateMessageAsync_WithValidRequest_UpdatesMessage()
    {
        await using var dbContext = CreateDbContext();

        var message = new Message
        {
            TicketId = 1,
            UserId = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Content = "Original",
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Messages.Add(message);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = CreateService(dbContext);

        var request = new UpdateMessageRequest
        {
            Content = "Updated content",
            Embedding = null
        };

        var result = await service.UpdateMessageAsync(message.Id, request, TestContext.Current.CancellationToken);

        Assert.True(result.Success);
        Assert.NotNull(result.Message);
        Assert.Equal("Updated content", result.Message.Content);
        Assert.True(result.Message.IsEdited);
        Assert.NotNull(result.Message.UpdatedAtUtc);

        var savedMessage = await dbContext.Messages.SingleAsync(cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal("Updated content", savedMessage.Content);
        Assert.True(savedMessage.IsEdited);
        Assert.NotNull(savedMessage.UpdatedAtUtc);
    }

    [Fact]
    public async Task UpdateMessageAsync_WithDeletedMessage_ReturnsFailure()
    {
        await using var dbContext = CreateDbContext();

        var message = new Message
        {
            TicketId = 1,
            UserId = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Content = "Original",
            CreatedAtUtc = DateTime.UtcNow,
            IsDeleted = true,
            DeletedAtUtc = DateTime.UtcNow
        };

        dbContext.Messages.Add(message);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = CreateService(dbContext);

        var request = new UpdateMessageRequest
        {
            Content = "Updated content",
            Embedding = null
        };

        var result = await service.UpdateMessageAsync(message.Id, request, TestContext.Current.CancellationToken);

        Assert.False(result.Success);
        Assert.Equal("Deleted messages cannot be edited.", result.ErrorMessage);
        Assert.Null(result.Message);
    }

    [Fact]
    public async Task SoftDeleteMessageAsync_WithExistingMessage_MarksMessageAsDeleted()
    {
        await using var dbContext = CreateDbContext();

        var message = new Message
        {
            TicketId = 1,
            UserId = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Content = "Delete me",
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Messages.Add(message);
        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = CreateService(dbContext);

        var result = await service.SoftDeleteMessageAsync(message.Id, TestContext.Current.CancellationToken);

        Assert.True(result.Deleted);
        Assert.Equal(1, result.TicketId);

        var savedMessage = await dbContext.Messages.SingleAsync(cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(savedMessage.IsDeleted);
        Assert.NotNull(savedMessage.DeletedAtUtc);
        Assert.NotNull(savedMessage.UpdatedAtUtc);
        Assert.Equal(string.Empty, savedMessage.Content);
    }

    [Fact]
    public async Task GetMessagesByTicketIdAsync_WithDeletedMessage_ReturnsDeletedPlaceholder()
    {
        await using var dbContext = CreateDbContext();

        dbContext.Messages.Add(new Message
        {
            TicketId = 1,
            UserId = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            Content = string.Empty,
            CreatedAtUtc = DateTime.UtcNow,
            IsDeleted = true,
            DeletedAtUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var service = CreateService(dbContext);

        var result = await service.GetMessagesByTicketIdAsync(1, 50, null, TestContext.Current.CancellationToken);

        Assert.Single(result.Items);
        Assert.Equal("[deleted]", result.Items[0].Content);
        Assert.True(result.Items[0].IsDeleted);
    }
}
