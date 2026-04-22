using CCP.Shared.ResultAbstraction;
using CustomerService.Domain.Entities;
using CustomerService.Sdk.Services;
using EmailService.Application.Interfaces;
using EmailService.Domain.Interfaces;
using EmailService.Domain.Models;
using EmailTemplates.Renderes;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using TicketService.Domain.Entities;
using TicketService.Domain.ResponseObjects;
using TicketService.Sdk.Dtos;



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
        public async Task<IResult> NotifyNewTicketCreation(
            [FromQuery] Guid customerId, [FromQuery] string ticketTitle,
            [FromQuery] TicketSdkDto ticket)
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
                    Body = $"A new support ticket has been created. Ticket ID: {ticket.Id}",
                    SenderAddress = _configuration.GetValue<string>("emailWorkerServiceUsername") ?? throw new InvalidOperationException("emailWorkerServiceUsername configuration value is required."),
                    RecipientAddress = customer.Email ?? "",
                    SentAt = DateTime.UtcNow,
                };

                await _ticketEmailService.SendTicketCreatedNotificationAsync(
                    recipientEmail: customer.Email ?? "",
                    ticketTitle: ticketTitle,
                    emailModel: emailModel,
                    ticket: ticket,
                    organizationName: organizationName,
                    expectedResponseTime: expectedResponseTime,
                    portalUrl: portalUrl);

                return Results.Accepted($"Email notification for ticket creation has been sent with ticket ID {ticket.Id} to {customer.Email}.");
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, title: "An error occurred while sending the email notification.");
            }
        }

        [HttpPost("status-change")]
        public async Task<IResult> NotifyTicketStatusChange(
            [FromQuery] Guid customerId, [FromQuery] string ticketTitle,
            [FromQuery] TicketSdkDto ticket, [FromQuery] string newStatus,
            [FromQuery] string oldStatus)
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
                    Body = $"The status of your support ticket (ID: {ticket.Id}) has changed from {oldStatus} to {newStatus}.",
                    SenderAddress = _configuration.GetValue<string>("emailWorkerServiceUsername") ?? throw new InvalidOperationException("emailWorkerServiceUsername configuration value is required."), 
                    RecipientAddress = customer.Email ?? "",
                    SentAt = DateTime.UtcNow,
                };

                await _ticketEmailService.SendTicketStatusChangeNotificationAsync(
                    recipientEmail: customer.Email ?? "",
                    ticketTitle: ticketTitle,
                    emailModel: emailModel,
                    ticket: ticket,
                    organizationName: organizationName,
                    oldStatusLabel: oldStatus,
                    portalUrl: portalUrl
                    );

                return Results.Accepted($"Email notification for status change has been sent for ticket ID {ticket.Id} to {customer.Email}.");
            }

            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, title: "An error occurred while sending the email notification.");
            }
        }

        [HttpPost("reply")]
        public async Task<IResult> NotifyTicketReply(
            [FromQuery] Guid customerId, [FromQuery] string ticketTitle,
            [FromQuery] TicketSdkDto ticket,[FromQuery] string agentName,
            [FromQuery] string agentRole)
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
                    Body = $"A support agent has replied to your ticket (ID: {ticket.Id}).",
                    SenderAddress = _configuration.GetValue<string>("emailWorkerServiceUsername") ?? throw new InvalidOperationException("emailWorkerServiceUsername configuration value is required."),
                    RecipientAddress = customer.Email ?? "",
                    ReceivedAt = DateTime.UtcNow,
                };

                await _ticketEmailService.SendTicketReplyNotificationAsync(
                    recipientEmail: customer.Email ?? "",
                    ticketTitle: ticketTitle,
                    emailModel: emailModel,
                    ticket: ticket,
                    customer: customer,
                    organizationName: organizationName,
                    agentName: agentName,
                    agentRole: agentRole,
                    replyUrl: replyUrl,
                    viewHistoryUrl: viewHistoryUrl
                    );

                return Results.Accepted($"Reply notification email for ticket ID {ticket.Id} has been sent to {customer.Email}.");
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, title: "An error occurred while sending the email notification.");
            }
        }
        [HttpPost("support/customer-replied")]
        public async Task<IResult> NotifySupportCustomerReply(
            [FromQuery] Guid customerId, [FromQuery] string agentEmail,
            [FromQuery] string agentName, [FromQuery] TicketSdkDto ticket,
            [FromQuery] string ticketTitle, [FromQuery] string replyContent)
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
                    Id = ticket.Id,
                    Subject = $"[Customer Reply] {ticketTitle}",
                    Body = replyContent,
                    SenderAddress = customer.Email ?? "",
                    RecipientAddress = agentEmail,
                    ReceivedAt = DateTime.UtcNow,
                };

                await _ticketEmailService.SendSupportCustomerReplyNotificationAsync(
                    recipientEmail: agentEmail,
                    emailModel: emailModel,
                    ticket: ticket,
                    customer: customer,
                    organizationName: organizationName,
                    replyUrl: replyUrl,
                    managementUrl: managementUrl,
                    viewHistoryUrl: viewHistoryUrl
                    );

                return Results.Accepted($"Support reply notification sent to {agentEmail} for ticket #{ticket.Id}.");
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, title: "An error occurred while sending the support reply notification.");
            }
        }

        [HttpPost("reply-to-email")]
        public async Task<IResult> NotifyReplyToEmail(
            [FromQuery] EmailReceived emailReceived, [FromQuery] EmailSent emailSent,
            [FromQuery] TicketSdkDto ticket, [FromQuery] string organizationName)
        {
            try
            {
                await _ticketEmailService.SendReplyToEmailAsync(
                    recipientEmail: emailReceived.RecipientAddress,
                    emailReceived: emailReceived,
                    emailSent: emailSent,
                    ticket: ticket,
                    organizationName: organizationName);
                return Results.Accepted($"Reply to email notification sent to {emailSent.RecipientAddress} for ticket #{ticket.Id}.");
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, title: "An error occurred while sending the reply to email notification.");
            }
        }
    }
}
