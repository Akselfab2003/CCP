using CCP.Shared.ResultAbstraction;
using CustomerService.Sdk.Services;
using EmailService.Application.Interfaces;
using EmailService.Domain.Interfaces;
using EmailService.Domain.Models;
using EmailTemplates.Renderes;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using TicketService.Sdk.Services.TicketSdk;



namespace EmailService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailSendingServiceController : ControllerBase
    {
        private readonly ITicketEmailService _ticketEmailService;
        private readonly ICustomerSdkService _customerSdkService;
        private readonly ITicketSdkService _ticketSdkService;
        private readonly IConfiguration _configuration;

        public EmailSendingServiceController(ITicketEmailService ticketEmailService, ICustomerSdkService customerSdkService, ITicketSdkService ticketSdkService, IConfiguration configuration)
        {
            _ticketEmailService = ticketEmailService;
            _customerSdkService = customerSdkService;
            _ticketSdkService = ticketSdkService;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IResult> NotifyNewTicketCreation(Guid customerId, int ticketId)
        {
            try
            {
                var ticketResult = await _ticketSdkService.GetTicketAsync(ticketId);

                if (ticketResult.IsFailure)
                    return Results.NotFound($"Ticket with ID {ticketId} not found.");

                var customerResult = await _customerSdkService.GetCustomerById(customerId);

                if (customerResult.IsFailure)
                    return Results.NotFound(new { message = $"Customer with ID {customerId} not found." });

                var ticket = ticketResult.Value;
                var customer = customerResult.Value;

                var portalUrl = _configuration.GetValue<string>("ApplicationUrls:CustomerPortal") ?? "#";
                var expectedResponseTime = _configuration.GetValue<string>("EmailSettings:ExpectedResponseTime") ?? "24 hours";
                var organizationName = _configuration.GetValue<string>("EmailSettings:OrganizationName") ?? "Support Team";

                var emailModel = new EmailSent
                {
                    Subject = ticket.Title,
                    Body = $"A new support ticket has been created. Ticket ID: {ticketId}",
                    SenderAddress = _configuration.GetValue<string>("EmailSettings:SenderAddress") ?? "noreply@support.com",
                    RecipientAddress = customer.Email ?? "",
                    SentAt = DateTime.UtcNow,
                };

                await _ticketEmailService.SendTicketCreatedNotificationAsync(
                    recipientEmail: customer.Email ?? "",
                    ticketTitle: ticket.Title,
                    emailModel: emailModel,
                    organizationName: organizationName,
                    expectedResponseTime: expectedResponseTime,
                    portalUrl: portalUrl);

                return Results.Accepted($"Email notification for ticket creation has been sent with ticket ID {ticketId} to {customer.Email}.");
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, title: "An error occurred while sending the email notification.");
            }
        }

        [HttpPost("status-change")]
        public async Task<IResult> NotifyTicketStatusChange(Guid customerId, int ticketId, string newStatus,string oldStatus)
        {
            try
            {
                var ticketResult = await _ticketSdkService.GetTicketAsync(ticketId);
                if (ticketResult.IsFailure)
                    return Results.NotFound($"Ticket with ID {ticketId} not found.");
                var customerResult = await _customerSdkService.GetCustomerById(customerId);
                if (customerResult.IsFailure)
                    return Results.NotFound(new { message = $"Customer with ID {customerId} not found." });

                var ticket = ticketResult.Value;
                var customer = customerResult.Value;

                var portalUrl = _configuration.GetValue<string>("ApplicationUrls:CustomerPortal") ?? "#";
                var organizationName = _configuration.GetValue<string>("EmailSettings:OrganizationName") ?? "Support Team";

                var emailModel = new EmailSent
                {
                    Subject = $"[Status Update] {ticket.Title} - Now {newStatus}",
                    Body = $"The status of your support ticket (ID: {ticketId}) has changed from {oldStatus} to {newStatus}.",
                    SenderAddress = _configuration.GetValue<string>("EmailSettings:SenderAddress") ?? "noreply@support.com",
                    RecipientAddress = customer.Email ?? "",
                    SentAt = DateTime.UtcNow,
                };

                await _ticketEmailService.SendTicketStatusChangeNotificationAsync(
                    recipientEmail: customer.Email ?? "",
                    ticketTitle: ticket.Title,
                    emailModel: emailModel,
                    organizationName: organizationName,
                    newStatus: newStatus,
                    newStatusLabel: newStatus,
                    oldStatusLabel: oldStatus,
                    updatedByAgent: "System",
                    agentNote: $"Your ticket status has been updated to {newStatus}.",
                    portalUrl: portalUrl,
                    reopenUrl: portalUrl
                    );

                return Results.Accepted($"Email notification for status change has been sent for ticket ID {ticketId} to {customer.Email}.");
            }

            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, title: "An error occurred while sending the email notification.");
            }
        }

        [HttpPost("reply")]
        public async Task<IResult> NotifyTicketReply(Guid customerId, int ticketId, string agentName, string agentRole, string replyContent)
        {
            try
            {
                var ticketResult = await _ticketSdkService.GetTicketAsync(ticketId);

                if (ticketResult.IsFailure)
                    return Results.NotFound($"Ticket with ID {ticketId} not found.");

                var customerResult = await _customerSdkService.GetCustomerById(customerId);

                if (customerResult.IsFailure)
                    return Results.NotFound(new { message = $"Customer with ID {customerId} not found." });

                var ticket = ticketResult.Value;
                var customer = customerResult.Value;

                var portalUrl = _configuration.GetValue<string>("ApplicationUrls:CustomerPortal") ?? "#";
                var replyUrl = _configuration.GetValue<string>("ApplicationUrls:ReplyToTicket") ?? "#";
                var viewHistoryUrl = _configuration.GetValue<string>("ApplicationUrls:ViewTicketHistory") ?? "#";
                var reopenUrl = _configuration.GetValue<string>("ApplicationUrls:ReopenTicket") ?? "#";
                var organizationName = _configuration.GetValue<string>("EmailSettings:OrganizationName") ?? "Support Team";

                var emailModel = new EmailReceived
                {
                    Subject = $"[Reply] {ticket.Title}",
                    Body = $"A support agent has replied to your ticket (ID: {ticketId}).",
                    SenderAddress = _configuration.GetValue<string>("EmailSettings:SenderAddress") ?? "noreply@support.com",
                    RecipientAddress = customer.Email ?? "",
                    ReceivedAt = DateTime.UtcNow,
                };

                await _ticketEmailService.SendTicketReplyNotificationAsync(
                    recipientEmail: customer.Email ?? "",
                    ticketTitle: ticket.Title,
                    emailModel: emailModel,
                    recipientName: customer.Name ?? "Customer",
                    organizationName: organizationName,
                    agentName: agentName,
                    agentRole: agentRole,
                    ticketStatus: ticket.Status.ToString(),
                    ticketStatusLabel: ticket.Status.ToString(),
                    replyUrl: replyUrl,
                    portalUrl: portalUrl,
                    viewHistoryUrl: viewHistoryUrl,
                    reopenUrl: reopenUrl);

                return Results.Accepted($"Reply notification email for ticket ID {ticketId} has been sent to {customer.Email}.");
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, title: "An error occurred while sending the email notification.");
            }
        }
    }
}
