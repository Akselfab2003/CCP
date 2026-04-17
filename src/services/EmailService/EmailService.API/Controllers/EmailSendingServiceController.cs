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
        private readonly IConfiguration _configuration;

        public EmailSendingServiceController(ITicketEmailService ticketEmailService, ICustomerSdkService customerSdkService, IConfiguration configuration)
        {
            _ticketEmailService = ticketEmailService;
            _customerSdkService = customerSdkService;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IResult> NotifyNewTicketCreation([FromQuery] Guid customerId, [FromQuery] string ticketTitle, [FromQuery] int ticketId)
        {
            try
            {
                var customerResult = await _customerSdkService.GetCustomerById(customerId);

                if (customerResult.IsFailure)
                    return Results.NotFound(new { message = $"Customer with ID {customerId} not found." });

                var customer = customerResult.Value;

                // Needs to be updated to have different URLs for different actions (view ticket, view ticket history, reopen ticket) instead of just the portal URL
                var portalUrl = _configuration.GetValue<string>("ApplicationUrls:CustomerPortal") ?? "#";
                var expectedResponseTime = _configuration.GetValue<string>("EmailSettings:ExpectedResponseTime") ?? "24 hours";
                var organizationName = _configuration.GetValue<string>("EmailSettings:OrganizationName") ?? "Support Team";

                var emailModel = new EmailSent
                {
                    Subject = ticketTitle,
                    Body = $"A new support ticket has been created. Ticket ID: {ticketId}",
                    SenderAddress = _configuration.GetValue<string>("emailWorkerServiceUsername") ?? throw new InvalidOperationException("emailWorkerServiceUsername configuration value is required."),
                    RecipientAddress = customer.Email ?? "",
                    SentAt = DateTime.UtcNow,
                };

                await _ticketEmailService.SendTicketCreatedNotificationAsync(
                    recipientEmail: customer.Email ?? "",
                    ticketTitle: ticketTitle,
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
        public async Task<IResult> NotifyTicketStatusChange([FromQuery] Guid customerId, [FromQuery] string ticketTitle, [FromQuery] int ticketId, [FromQuery] string newStatus, [FromQuery] string oldStatus)
        {
            try
            {
                var customerResult = await _customerSdkService.GetCustomerById(customerId);
                if (customerResult.IsFailure)
                    return Results.NotFound(new { message = $"Customer with ID {customerId} not found." });

                var customer = customerResult.Value;

                // Needs to be updated to have different URLs for different actions (view ticket, view ticket history, reopen ticket) instead of just the portal URL
                // These URLs can be used in the email template to direct customers to the appropriate pages in the customer portal
                var portalUrl = _configuration.GetValue<string>("ApplicationUrls:CustomerPortal") ?? "#";
                var organizationName = _configuration.GetValue<string>("EmailSettings:OrganizationName") ?? "Support Team";

                var emailModel = new EmailSent
                {
                    Subject = $"[Status Update] {ticketTitle} - Now {newStatus}",
                    Body = $"The status of your support ticket (ID: {ticketId}) has changed from {oldStatus} to {newStatus}.",
                    SenderAddress = _configuration.GetValue<string>("emailWorkerServiceUsername") ?? throw new InvalidOperationException("emailWorkerServiceUsername configuration value is required."), 
                    RecipientAddress = customer.Email ?? "",
                    SentAt = DateTime.UtcNow,
                };

                await _ticketEmailService.SendTicketStatusChangeNotificationAsync(
                    recipientEmail: customer.Email ?? "",
                    ticketTitle: ticketTitle,
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
        public async Task<IResult> NotifyTicketReply([FromQuery] Guid customerId, [FromQuery] string ticketTitle, [FromQuery] int ticketId,[FromQuery] string ticketStatus,[FromQuery] string ticketStatusLabel, [FromQuery] string agentName, [FromQuery] string agentRole, [FromQuery] string replyContent)
        {
            try
            {

                var customerResult = await _customerSdkService.GetCustomerById(customerId);

                if (customerResult.IsFailure)
                    return Results.NotFound(new { message = $"Customer with ID {customerId} not found." });

                var customer = customerResult.Value;

                // Needs to be updated to have different URLs for different actions (view ticket, view ticket history, reopen ticket) instead of just the portal URL
                var portalUrl = _configuration.GetValue<string>("ApplicationUrls:CustomerPortal") ?? "#";
                var replyUrl = _configuration.GetValue<string>("ApplicationUrls:ReplyToTicket") ?? "#";
                var viewHistoryUrl = _configuration.GetValue<string>("ApplicationUrls:ViewTicketHistory") ?? "#";
                var reopenUrl = _configuration.GetValue<string>("ApplicationUrls:ReopenTicket") ?? "#";
                var organizationName = _configuration.GetValue<string>("EmailSettings:OrganizationName") ?? "Support Team";

                var emailModel = new EmailReceived
                {
                    Subject = $"[Reply] {ticketTitle}",
                    Body = $"A support agent has replied to your ticket (ID: {ticketId}).",
                    SenderAddress = _configuration.GetValue<string>("emailWorkerServiceUsername") ?? throw new InvalidOperationException("emailWorkerServiceUsername configuration value is required."),
                    RecipientAddress = customer.Email ?? "",
                    ReceivedAt = DateTime.UtcNow,
                };

                await _ticketEmailService.SendTicketReplyNotificationAsync(
                    recipientEmail: customer.Email ?? "",
                    ticketTitle: ticketTitle,
                    emailModel: emailModel,
                    recipientName: customer.Name ?? "Customer",
                    organizationName: organizationName,
                    agentName: agentName,
                    agentRole: agentRole,
                    ticketStatus: ticketStatus,
                    ticketStatusLabel: ticketStatusLabel,
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
        [HttpPost("support/customer-replied")]
        public async Task<IResult> NotifySupportCustomerReply(
            [FromQuery] Guid customerId,
            [FromQuery] string agentEmail,      // the support agent assigned to the ticket
            [FromQuery] string agentName,
            [FromQuery] int ticketId,
            [FromQuery] string ticketTitle,
            [FromQuery] string ticketStatus,
            [FromQuery] string ticketStatusLabel,
            [FromQuery] string replyContent)
        {
            try
            {
                var customerResult = await _customerSdkService.GetCustomerById(customerId);
                if (customerResult.IsFailure)
                    return Results.NotFound(new { message = $"Customer with ID {customerId} not found." });

                var customer = customerResult.Value;

                var replyUrl = _configuration.GetValue<string>("ApplicationUrls:ReplyToTicket") ?? "#";
                var managementUrl = _configuration.GetValue<string>("ApplicationUrls:ManageTicket") ?? "#";
                var viewHistoryUrl = _configuration.GetValue<string>("ApplicationUrls:ViewTicketHistory") ?? "#";
                var organizationName = _configuration.GetValue<string>("EmailSettings:OrganizationName") ?? "Support Team";

                var emailModel = new EmailReceived
                {
                    Id = ticketId,
                    Subject = $"[Customer Reply] {ticketTitle}",
                    Body = replyContent,
                    SenderAddress = customer.Email ?? "",
                    RecipientAddress = agentEmail,
                    ReceivedAt = DateTime.UtcNow,
                };

                await _ticketEmailService.SendSupportCustomerReplyNotificationAsync(
                    recipientEmail: agentEmail,
                    emailModel: emailModel,
                    customerName: customer.Name ?? "Customer",
                    customerEmail: customer.Email ?? "",
                    organizationName: organizationName,
                    ticketStatus: ticketStatus,
                    ticketStatusLabel: ticketStatusLabel,
                    replyUrl: replyUrl,
                    managementUrl: managementUrl,
                    viewHistoryUrl: viewHistoryUrl);

                return Results.Accepted($"Support reply notification sent to {agentEmail} for ticket #{ticketId}.");
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, title: "An error occurred while sending the support reply notification.");
            }
        }
        [HttpPost("support/new-ticket")]
        public async Task<IResult> NotifySupportNewTicket(
            [FromQuery] Guid customerId,
            [FromQuery] string supportTeamEmail,   // e.g. "support@yourcompany.com"
            [FromQuery] string ticketTitle,
            [FromQuery] int ticketId,
            [FromQuery] string ticketBody)
        {
            try
            {
                var customerResult = await _customerSdkService.GetCustomerById(customerId);
                if (customerResult.IsFailure)
                    return Results.NotFound(new { message = $"Customer with ID {customerId} not found." });

                var customer = customerResult.Value;

                var managementUrl = _configuration.GetValue<string>("ApplicationUrls:ManageTicket") ?? "#";
                var expectedResponse = _configuration.GetValue<string>("EmailSettings:ExpectedResponseTime") ?? "24 hours";
                var organizationName = _configuration.GetValue<string>("EmailSettings:OrganizationName") ?? "Support Team";

                var emailModel = new EmailSent
                {
                    Id = ticketId,
                    Subject = ticketTitle,
                    Body = ticketBody,
                    SenderAddress = customer.Email ?? "",
                    RecipientAddress = supportTeamEmail,
                    SentAt = DateTime.UtcNow,
                };

                await _ticketEmailService.SendSupportNewTicketNotificationAsync(
                    recipientEmail: supportTeamEmail,
                    emailModel: emailModel,
                    customerEmail: customer.Email ?? "",
                    organizationName: organizationName,
                    expectedResponseTime: expectedResponse,
                    managementUrl: managementUrl);

                return Results.Accepted($"Support new-ticket notification sent to {supportTeamEmail} for ticket #{ticketId}.");
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, title: "An error occurred while sending the support new-ticket notification.");
            }
        }
    }
}
