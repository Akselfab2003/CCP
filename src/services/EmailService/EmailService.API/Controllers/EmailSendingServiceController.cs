using CCP.Shared.AuthContext;
using CCP.Shared.ResultAbstraction;
using CCP.Shared.ValueObjects;
using CustomerService.Sdk.Services;
using EmailService.Application.Interfaces;
using EmailService.Domain.Models;
using MessagingService.Sdk.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketService.Sdk.Services.Ticket;



namespace EmailService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EmailSendingServiceController : ControllerBase
    {
        private readonly ITicketEmailService _ticketEmailService;
        private readonly ITicketService _ticketService;
        private readonly ICustomerSdkService _customerSdkService;
        private readonly IMessageSdkService _messageSdkService;
        private readonly IConfiguration _configuration;
        private readonly ICurrentUser _currentUser;
        private readonly ServiceAccountOverrider _serviceAccountOverrider;
        public EmailSendingServiceController(ITicketEmailService ticketEmailService, ICustomerSdkService customerSdkService, IMessageSdkService messageSdkService, IConfiguration configuration, ICurrentUser currentUser, ServiceAccountOverrider serviceAccountOverrider, ITicketService ticketService)
        {
            _ticketEmailService = ticketEmailService;
            _customerSdkService = customerSdkService;
            _messageSdkService = messageSdkService;
            _configuration = configuration;
            _currentUser = currentUser;
            _serviceAccountOverrider = serviceAccountOverrider;
            _ticketService = ticketService;
        }

        [HttpPost]
        public async Task<IResult> NotifyNewTicketCreation(
            [FromQuery] Guid customerId, [FromQuery] string ticketTitle,
            [FromQuery] int TicketId, [FromQuery] string TicketStatus, [FromQuery] TicketOrigin origin, [FromQuery] Guid OrgId, [FromQuery] string orgName)
        {
            try
            {
                _serviceAccountOverrider.SetOrganizationId(_currentUser.OrganizationId);

                if (!Enum.TryParse(TicketStatus, out TicketStatus parsedStatus))
                {
                    return Results.BadRequest(new { message = $"Invalid ticket status value: {TicketStatus}" });
                }
                var customerResult = await _customerSdkService.GetCustomerById(customerId);

                if (customerResult.IsFailure)
                    return Results.NotFound(new { message = $"Customer with ID {customerId} not found." });

                var customer = customerResult.Value;

                var portalUrl = $"{_configuration.GetValue<string>("emailPortalUrl")}/tickets/{TicketId}";
                var expectedResponseTime = "24 Hours";
                var organizationName = orgName;

                var emailModel = new EmailSent
                {
                    Subject = ticketTitle,
                    Body = $"A new support ticket has been created. Ticket ID: {TicketId}",
                    SenderAddress = _configuration.GetValue<string>("emailWorkerServiceUsername") ?? throw new InvalidOperationException("emailWorkerServiceUsername configuration value is required."),
                    RecipientAddress = customer.Email ?? "",
                    SentAt = DateTime.UtcNow,
                };

                await _ticketEmailService.SendTicketCreatedNotificationAsync(
                    recipientEmail: customer.Email ?? "",
                    ticketTitle: ticketTitle,
                    emailModel: emailModel,
                    ticketId: TicketId,
                    ticketStatus: parsedStatus,
                    organizationName: organizationName,
                    expectedResponseTime: expectedResponseTime,
                    portalUrl: portalUrl,
                    origin: origin,
                    OrgId: customer.OrganizationId);

                return Results.Accepted($"Email notification for ticket creation has been sent with ticket ID {TicketId} to {customer.Email}.");
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, title: "An error occurred while sending the email notification.");
            }
        }

        [HttpPost("status-change")]
        public async Task<IResult> NotifyTicketStatusChange(
            [FromQuery] Guid customerId, [FromQuery] string ticketTitle,
            [FromQuery] int TicketId, [FromQuery] string newStatus,
            [FromQuery] string oldStatus, [FromQuery] TicketOrigin origin, [FromQuery] string orgName)
        {
            try
            {
                _serviceAccountOverrider.SetOrganizationId(_currentUser.OrganizationId);

                if (!Enum.TryParse(newStatus, out TicketStatus parsedStatus))
                {
                    return Results.BadRequest(new { message = $"Invalid ticket status value: {newStatus}" });
                }
                var customerResult = await _customerSdkService.GetCustomerById(customerId);
                if (customerResult.IsFailure)
                    return Results.NotFound(new { message = $"Customer with ID {customerId} not found." });

                var customer = customerResult.Value;

                var portalUrl = $"{_configuration.GetValue<string>("emailPortalUrl")}/tickets/{TicketId}";
                var organizationName = orgName;

                var emailModel = new EmailSent
                {
                    Subject = $"[Status Update] {ticketTitle} - Now {newStatus}",
                    Body = $"The status of your support ticket (ID: {TicketId}) has changed from {oldStatus} to {newStatus}.",
                    SenderAddress = _configuration.GetValue<string>("emailWorkerServiceUsername") ?? throw new InvalidOperationException("emailWorkerServiceUsername configuration value is required."),
                    RecipientAddress = customer.Email ?? "",
                    SentAt = DateTime.UtcNow,
                };

                await _ticketEmailService.SendTicketStatusChangeNotificationAsync(
                    recipientEmail: customer.Email ?? "",
                    ticketTitle: ticketTitle,
                    emailModel: emailModel,
                    ticketId: TicketId,
                    ticketStatus: parsedStatus,
                    organizationName: organizationName,
                    oldStatusLabel: oldStatus,
                    portalUrl: portalUrl,
                    origin: origin,
                    OrgId: customer.OrganizationId
                    );

                return Results.Accepted($"Email notification for status change has been sent for ticket ID {TicketId} to {customer.Email}.");
            }

            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, title: "An error occurred while sending the email notification.");
            }
        }

        [HttpPost("reply")]
        public async Task<IResult> NotifyTicketReply(
            [FromQuery] int TicketId, [FromQuery] string TicketStatus, [FromQuery] string agentName,
            [FromQuery] string agentRole, [FromQuery] TicketOrigin origin, [FromQuery] string orgName)
        {
            try
            {
                _serviceAccountOverrider.SetOrganizationId(_currentUser.OrganizationId);

                var ticketResult = await _ticketService.GetTicket(TicketId);

                if (ticketResult == null)
                    return Results.NotFound(new { message = $"Ticket with ID {TicketId} not found." });

                var ticket = ticketResult.Value;

                if (!ticket.CustomerId.HasValue)
                    return Results.BadRequest(new { message = $"Ticket with ID {TicketId} does not have an associated customer." });

                if (origin == TicketOrigin.Manual)
                {
                    if (!Enum.TryParse(TicketStatus, out TicketStatus parsedStatus))
                    {
                        return Results.BadRequest(new { message = $"Invalid ticket status value: {TicketStatus}" });
                    }

                    var customerResult = await _customerSdkService.GetCustomerById(ticket.CustomerId.Value);

                    if (customerResult.IsFailure)
                        return Results.NotFound(new { message = $"Customer with ID {ticket.CustomerId.Value} not found." });

                    var customer = customerResult.Value;

                    var replyUrl = $"{_configuration.GetValue<string>("emailPortalUrl")}/tickets/{TicketId}";
                    var viewHistoryUrl = $"{_configuration.GetValue<string>("emailPortalUrl")}/tickets/{TicketId}";
                    var organizationName = orgName;

                    var emailModel = new EmailReceived
                    {
                        Subject = $"[Reply] {ticket.Title}",
                        Body = $"A support agent has replied to your ticket (ID: {TicketId}).",
                        SenderAddress = _configuration.GetValue<string>("emailWorkerServiceUsername") ?? throw new InvalidOperationException("emailWorkerServiceUsername configuration value is required."),
                        RecipientAddress = customer.Email ?? "",
                        ReceivedAt = DateTime.UtcNow,
                    };

                    await _ticketEmailService.SendTicketReplyNotificationAsync(
                        recipientEmail: customer.Email ?? "",
                        ticketTitle: ticket.Title,
                        emailModel: emailModel,
                        ticketId: TicketId,
                        ticketStatus: parsedStatus,
                        customer: customer,
                        organizationName: organizationName,
                        agentName: agentName,
                        agentRole: agentRole,
                        replyUrl: replyUrl,
                        viewHistoryUrl: viewHistoryUrl,
                        origin: origin,
                        OrgId: ticket.OrganizationId
                        );
                }
                else if (origin == TicketOrigin.Email)
                {
                    if (!Enum.TryParse(TicketStatus, out TicketStatus parsedStatus))
                    {
                        return Results.BadRequest(new { message = $"Invalid ticket status value: {TicketStatus}" });
                    }
                    var customerResult = await _customerSdkService.GetCustomerById(ticket.CustomerId.Value);
                    if (customerResult.IsFailure)
                        return Results.NotFound(new { message = $"Customer with ID {ticket.CustomerId.Value} not found." });

                    var customer = customerResult.Value;

                    var organizationName = orgName;

                    var messagePageResult = await _messageSdkService.GetMessagesByTicketIdAsync(TicketId);

                    if (messagePageResult.IsFailure)
                    {
                        return messagePageResult.ToProblemDetails();
                    }

                    var messages = messagePageResult.Value.Items;

                    if (messages == null || !messages.Any())
                    {
                        return Results.Problem(detail: $"No messages found for ticket ID {TicketId}.", title: "Messages Not Found");
                    }

                    var emailModel = new EmailSent()
                    {
                        Subject = $"[Reply] {ticket.Title}",
                        Body = $"There is a new reply to your ticket (ID: {TicketId}).",
                        SenderAddress = _configuration.GetValue<string>("emailWorkerServiceUsername") ?? throw new InvalidOperationException("emailWorkerServiceUsername configuration value is required."),
                        RecipientAddress = customer.Email ?? "",
                        SentAt = DateTime.UtcNow,
                    };

                    await _ticketEmailService.SendReplyToEmailAsync(
                        recipientEmail: customer.Email ?? "",
                        email: emailModel,
                        messages: messages.ToList(),
                        ticketId: TicketId,
                        customerId: customer.Id,
                        OrgId: ticket.OrganizationId,
                        organizationName: organizationName,
                        origin: origin,
                        ticketStatus: parsedStatus
                        );
                }

                return Results.Accepted($"Reply notification email for ticket ID {TicketId} has been sent.");

            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, title: "An error occurred while sending the email notification.");
            }
        }
        [HttpPost("support/customer-replied")]
        public async Task<IResult> NotifySupportCustomerReply(
            [FromQuery] Guid customerId, [FromQuery] string agentEmail,
            [FromQuery] string agentName, [FromQuery] int TicketId, [FromQuery] string TicketStatus,
            [FromQuery] string ticketTitle, [FromQuery] string replyContent, [FromQuery] TicketOrigin origin, [FromQuery] string orgName)
        {
            try
            {
                _serviceAccountOverrider.SetOrganizationId(_currentUser.OrganizationId);

                if (!Enum.TryParse(TicketStatus, out TicketStatus parsedStatus))
                {
                    return Results.BadRequest(new { message = $"Invalid ticket status value: {TicketStatus}" });
                }

                var customerResult = await _customerSdkService.GetCustomerById(customerId);
                if (customerResult.IsFailure)
                    return Results.NotFound(new { message = $"Customer with ID {customerId} not found." });

                var customer = customerResult.Value;

                var managementUrl = $"{_configuration.GetValue<string>("emailPortalUrl")}/tickets/{TicketId}";
                var replyUrl = $"{_configuration.GetValue<string>("emailPortalUrl")}/tickets/{TicketId}";
                var viewHistoryUrl = $"{_configuration.GetValue<string>("emailPortalUrl")}/tickets/{TicketId}";
                var organizationName = orgName;

                var emailModel = new EmailReceived
                {
                    Id = TicketId,
                    Subject = $"[Customer Reply] {ticketTitle}",
                    Body = replyContent,
                    SenderAddress = customer.Email ?? "",
                    RecipientAddress = agentEmail,
                    ReceivedAt = DateTime.UtcNow,
                };

                await _ticketEmailService.SendSupportCustomerReplyNotificationAsync(
                    recipientEmail: agentEmail,
                    emailModel: emailModel,
                    ticketId: TicketId,
                    ticketStatus: parsedStatus,
                    customer: customer,
                    organizationName: organizationName,
                    replyUrl: replyUrl,
                    managementUrl: managementUrl,
                    viewHistoryUrl: viewHistoryUrl,
                    origin: origin,
                    OrgId: customer.OrganizationId
                    );

                return Results.Accepted($"Support reply notification sent to {agentEmail} for ticket #{TicketId}.");
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, title: "An error occurred while sending the support reply notification.");
            }
        }
    }
}
