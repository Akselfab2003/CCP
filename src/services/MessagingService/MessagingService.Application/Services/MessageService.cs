using System.Runtime.CompilerServices;
using CCP.Shared.AuthContext;
using CCP.Shared.UIContext;
using CCP.Shared.ValueObjects;
using EmailService.Sdk.Services;
using IdentityService.Sdk.Services.Tenant;
using IdentityService.Sdk.Services.User;
using MessagingService.Domain.Contracts;
using MessagingService.Domain.Entities;
using MessagingService.Domain.Interfaces;
using MessagingService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pgvector;
using TicketService.Sdk.Dtos;
using TicketService.Sdk.Services.Ticket;

namespace MessagingService.Application.Services;

public class MessageService : IMessageService
{
    private const int EmbeddingDimensions = 1536;
    private const int MaxContentLength = 2000;
    private const int DefaultPageSize = 50;
    private const int MaxPageSize = 100;

    private readonly MessagingDbContext _dbContext;
    private readonly IMessageAccessValidator _messageAccessValidator;
    private readonly ITicketService _ticketService;
    private readonly IEmailSdkService _emailSdkService;
    private readonly ServiceAccountOverrider _serviceAccountOverrider;
    private readonly ILogger<MessageService> _logger;
    private readonly ITenantService _tenantService;
    private readonly IUserService _userService;

    public MessageService(MessagingDbContext dbContext,ITenantService tenantService,IUserService userService, IMessageAccessValidator messageAccessValidator, ServiceAccountOverrider serviceAccountOverrider, IEmailSdkService emailSdkService, ITicketService ticketService, ILogger<MessageService> logger)
    {
        _dbContext = dbContext;
        _messageAccessValidator = messageAccessValidator;
        _emailSdkService = emailSdkService;
        _ticketService = ticketService;
        _serviceAccountOverrider = serviceAccountOverrider;
        _logger = logger;
        _tenantService = tenantService;
        _userService = userService;
    }

    public async Task<MessageServiceResult> CreateMessageAsync(
        CreateMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        _serviceAccountOverrider.SetOrganizationId(request.OrganizationId);
        if (request.TicketId <= 0)
            return MessageServiceResult.Failed("TicketId must be greater than 0.");

        var ticket = await _ticketService.GetTicket(request.TicketId);
        if (ticket is null || ticket.IsFailure)
            return MessageServiceResult.Failed("Ticket not found.");

        if (request.OrganizationId == Guid.Empty)
            return MessageServiceResult.Failed("OrganizationId is required.");

        if (request.UserId.HasValue && request.UserId.Value == Guid.Empty)
            return MessageServiceResult.Failed("UserId cannot be an empty GUID.");

        if (string.IsNullOrWhiteSpace(request.Content))
            return MessageServiceResult.Failed("Content is required.");

        var trimmedContent = request.Content.Trim();

        if (trimmedContent.Length > MaxContentLength)
            return MessageServiceResult.Failed($"Content cannot exceed {MaxContentLength} characters.");

        if (request.Embedding is not null && request.Embedding.Length != EmbeddingDimensions)
            return MessageServiceResult.Failed($"Embedding must contain exactly {EmbeddingDimensions} values.");

        var canSend = await _messageAccessValidator.CanSendMessageAsync(
            request.TicketId,
            request.OrganizationId,
            request.UserId,
            cancellationToken);

        if (!canSend)
            return MessageServiceResult.Failed("User is not allowed to send messages for this ticket.");

        var message = new Message
        {
            TicketId = request.TicketId,
            UserId = request.UserId,
            OrganizationId = request.OrganizationId,
            Content = trimmedContent,
            CreatedAtUtc = DateTime.UtcNow,
            IsEdited = false,
            IsDeleted = false,
            IsInternalNote = request.IsInternalNote,
            Embedding = request.Embedding is null ? null : new Vector(request.Embedding),
            AttachmentUrl = request.AttachmentUrl,
            AttachmentFileName = request.AttachmentFileName,
            AttachmentContentType = request.AttachmentContentType
        };

        _dbContext.Messages.Add(message);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await ForwardMessageToServices(ticket.Value, message);

        _ = _ticketService.RecordMessageSentAsync(
            message.TicketId,
            message.UserId,
            message.Content.Length > 120 ? message.Content[..120] : message.Content

        ).ContinueWith(t =>
                {
                    if (t.IsFaulted)
                        _logger.LogWarning("Failed to record message history for ticket {TicketId}: {Error}",
                    message.TicketId, t.Exception?.Message);
                }, TaskScheduler.Default);

        return MessageServiceResult.Succeeded(MapToResponse(message));
    }

    public async Task<PagedMessagesResponse> GetMessagesByTicketIdAsync(
        int ticketId,
        int limit,
        int? beforeMessageId,
        CancellationToken cancellationToken = default)
    {
        if (ticketId <= 0)
        {
            return new PagedMessagesResponse();
        }

        var normalizedLimit = NormalizeLimit(limit);

        var query = _dbContext.Messages
            .AsNoTracking()
            .Where(m => m.TicketId == ticketId);

        if (beforeMessageId.HasValue)
        {
            query = query.Where(m => m.Id < beforeMessageId.Value);
        }

        var items = await query
            .OrderByDescending(m => m.Id)
            .Take(normalizedLimit + 1)
            .Select(m => new MessageResponse
            {
                Id = m.Id,
                TicketId = m.TicketId,
                UserId = m.UserId,
                OrganizationId = m.OrganizationId,
                Content = m.IsDeleted ? "[deleted]" : m.Content,
                CreatedAtUtc = m.CreatedAtUtc,
                UpdatedAtUtc = m.UpdatedAtUtc,
                IsEdited = m.IsEdited,
                IsDeleted = m.IsDeleted,
                DeletedAtUtc = m.DeletedAtUtc,
                IsInternalNote = m.IsInternalNote,
                Embedding = m.Embedding == null ? null : m.Embedding.ToArray(),
                AttachmentUrl = m.AttachmentUrl,
                AttachmentFileName = m.AttachmentFileName,
                AttachmentContentType = m.AttachmentContentType
            })
            .ToListAsync(cancellationToken);

        var hasMore = items.Count > normalizedLimit;

        if (hasMore)
        {
            items = items.Take(normalizedLimit).ToList();
        }

        items = items.OrderBy(m => m.Id).ToList();

        return new PagedMessagesResponse
        {
            Items = items,
            HasMore = hasMore
        };
    }

    public async Task<MessageResponse?> GetMessageByIdAsync(int messageId, CancellationToken cancellationToken = default)
    {
        if (messageId <= 0)
            return null;

        var message = await _dbContext.Messages
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);

        return message is null ? null : MapToResponse(message);
    }

    public async Task<MessageServiceResult> UpdateMessageAsync(
        int messageId,
        UpdateMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        if (messageId <= 0)
            return MessageServiceResult.Failed("MessageId must be greater than 0.");

        var message = await _dbContext.Messages.FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);

        if (message is null)
            return MessageServiceResult.Failed("Message not found.");

        if (message.IsDeleted)
            return MessageServiceResult.Failed("Deleted messages cannot be edited.");

        if (string.IsNullOrWhiteSpace(request.Content))
            return MessageServiceResult.Failed("Content is required.");

        var trimmedContent = request.Content.Trim();

        if (trimmedContent.Length > MaxContentLength)
            return MessageServiceResult.Failed($"Content cannot exceed {MaxContentLength} characters.");

        if (request.Embedding is not null && request.Embedding.Length != EmbeddingDimensions)
            return MessageServiceResult.Failed($"Embedding must contain exactly {EmbeddingDimensions} values.");

        message.Content = trimmedContent;
        message.IsEdited = true;
        message.UpdatedAtUtc = DateTime.UtcNow;
        message.Embedding = request.Embedding is null ? null : new Vector(request.Embedding);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MessageServiceResult.Succeeded(MapToResponse(message));
    }

    public async Task<(bool Deleted, int? TicketId)> SoftDeleteMessageAsync(int messageId, CancellationToken cancellationToken = default)
    {
        if (messageId <= 0)
            return (false, null);

        var message = await _dbContext.Messages.FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);

        if (message is null || message.IsDeleted)
            return (false, null);

        message.IsDeleted = true;
        message.DeletedAtUtc = DateTime.UtcNow;
        message.UpdatedAtUtc = DateTime.UtcNow;
        message.Content = string.Empty;
        message.Embedding = null;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return (true, message.TicketId);
    }

    private static int NormalizeLimit(int limit)
    {
        if (limit <= 0) return DefaultPageSize;
        if (limit > MaxPageSize) return MaxPageSize;
        return limit;
    }

    private static MessageResponse MapToResponse(Message message)
    {
        return new MessageResponse
        {
            Id = message.Id,
            TicketId = message.TicketId,
            UserId = message.UserId,
            OrganizationId = message.OrganizationId,
            Content = message.IsDeleted ? "[deleted]" : message.Content,
            CreatedAtUtc = message.CreatedAtUtc,
            UpdatedAtUtc = message.UpdatedAtUtc,
            IsEdited = message.IsEdited,
            IsDeleted = message.IsDeleted,
            DeletedAtUtc = message.DeletedAtUtc,
            IsInternalNote = message.IsInternalNote,
            Embedding = message.Embedding?.ToArray(),
            AttachmentUrl = message.AttachmentUrl,
            AttachmentFileName = message.AttachmentFileName,
            AttachmentContentType = message.AttachmentContentType
        };
    }


    private async Task ForwardMessageToServices(TicketSdkDto ticket, Message msg)
    {
        try
        {
            _serviceAccountOverrider.SetOrganizationId(ticket.OrganizationId);

            var tenantResult = await _tenantService.GetTenantDetailsAsync(ticket.OrganizationId);
            var userId = msg.UserId;
            UserRole userRole = UserRole.Customer;
            string customerName = "";

            if (userId.HasValue)
            {
                var userRoleResult = await _userService.GetUserDetailsAsync(userId.Value);
                userRole = userRoleResult.Value.groups
                    .Select(s => s switch
                    {
                        "Admin" => UserRole.Admin,
                        "Manager" => UserRole.Manager,
                        "Supporter" => UserRole.Supporter,
                        "Customer" => UserRole.Customer,
                        _ => UserRole.Customer
                    })
                    .FirstOrDefault();
                customerName = userRoleResult.Value.name;
            }
            string agentName = "";
            string agentEmail = "";

            var agentInfo = ticket.AssignedUserId;
            if (agentInfo.HasValue)
            {
                var agentResult = await _userService.GetUserDetailsAsync(agentInfo.Value);
                agentName = agentResult.Value.name;
                agentEmail = agentResult.Value.email;
            }

            switch (ticket.Origin)
            {
                case TicketOrigin.Manual:
                    if (userRole == UserRole.Supporter || userRole == UserRole.Admin || userRole == UserRole.Manager)
                    {
                        await _emailSdkService.NotifyTicketRepliedAsync(
                            ticketId: ticket.Id,
                            status: (TicketStatus)ticket.Status,
                            origin: ticket.Origin,
                            agentName: customerName,
                            agentRole: userRole.ToString(),
                            orgName: tenantResult.Value.Name);
                    }
                    else
                    {
                        if (ticket.CustomerId.HasValue && ticket.CustomerId.Value != Guid.Empty)
                        {
                            await _emailSdkService.NotifySupportCustomerReplyAsync(
                                customerId: ticket.CustomerId.Value,
                                agentEmail: agentEmail,
                                agentName: agentName,
                                ticketId: ticket.Id,
                                ticketTitle: ticket.Title,
                                ticketStatus: (TicketStatus)ticket.Status,
                                replyContent: msg.Content,
                                orgName: tenantResult.Value.Name);

                            _logger.LogInformation("Customer {UserId} sent a message on ticket {TicketId}. Support team notified.", agentName, ticket.Id);
                        }
                        else
                        {
                            _logger.LogWarning("Cannot notify support team: CustomerId is null or empty for ticket {TicketId}", ticket.Id);
                        }
                    }
                    break;

                case TicketOrigin.Email:
                    if (msg.UserId.HasValue)
                    {
                        await _emailSdkService.NotifyTicketRepliedAsync(
                            ticketId: ticket.Id,
                            status: (TicketStatus)ticket.Status,
                            origin: ticket.Origin,
                            agentName: agentName,
                            agentRole: userRole.ToString(),
                            orgName: tenantResult.Value.Name);
                    }

                    break;

                case TicketOrigin.Chatbot:
                    break;

                default:
                    break;
            }
        }
        catch (Exception)
        {

        }
    }
}
