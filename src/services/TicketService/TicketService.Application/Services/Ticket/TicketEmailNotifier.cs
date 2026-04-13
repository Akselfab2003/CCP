using System;
using System.Collections.Generic;
using System.Text;
using CustomerService.Api.DB.Models;
using CustomerService.Application.Services;
using EmailService.Application.Interfaces;
using EmailService.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TicketService.Application.Services.Ticket
{
    public class TicketEmailNotifier : ITicketEmailNotifier
    {
        private readonly ITicketQueries _ticketQueries;
        private readonly ITicketEmailService _ticketEmailService;
        private readonly ICustomerService _customerService;
        private readonly ILogger<TicketEmailNotifier> _logger;
        private readonly IConfiguration _configuration;

        public TicketEmailNotifier(
            ITicketQueries ticketQueries,
            ITicketEmailService ticketEmailService,
            ICustomerService customerService,
            ILogger<TicketEmailNotifier> logger,
            IConfiguration configuration)
        {
            _ticketQueries = ticketQueries;
            _ticketEmailService = ticketEmailService;
            _customerService = customerService;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task NotifyTicketCreatedAsync(int ticketId)
        {
            try
            {
                var ticketResult = await _ticketQueries.GetTicket(ticketId);
                if (ticketResult.IsFailure)
                {
                    _logger.LogWarning($"Could not find ticket {ticketId} to send created email notification");
                    return;
                }

                var ticket = ticketResult.Value;

                if (!ticket.CustomerId.HasValue)
                {
                    _logger.LogWarning($"Ticket {ticketId} has no customer ID, skipping email notification");
                    return;
                }

                var portalUrl = _configuration.GetValue<string>("ApplicationUrls:CustomerPortal") ?? "#";
                var expectedResponseTime = _configuration.GetValue<string>("EmailSettings:ExpectedResponseTime") ?? "24 hours";
                var organizationName = _configuration.GetValue<string>("EmailSettings:OrganizationName") ?? "Support Team";

                var customer = await _customerService.GetCustomerById(ticket.CustomerId.Value);
                if (customer == null)
                {
                    _logger.LogWarning($"Customer not found for ticket {ticketId}");
                    return;
                }

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
                    portalUrl: portalUrl
                    );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error notifying ticket created for ticket {ticketId}");
            }
        }

        public async Task NotifyTicketStatusChangedAsync(
            int ticketId,
            string oldStatus,
            string newStatus,
            string agentName,
            string agentRole,
            string agentNote)
        {
            try
            {
                var ticketResult = await _ticketQueries.GetTicket(ticketId);
                if (ticketResult.IsFailure)
                {
                    _logger.LogWarning($"Could not find ticket {ticketId} to send status change email notification");
                    return;
                }

                var ticket = ticketResult.Value;

                if (!ticket.CustomerId.HasValue)
                {
                    _logger.LogWarning($"Ticket {ticketId} has no customer ID, skipping email notification");
                    return;
                }

                var portalUrl = _configuration.GetValue<string>("ApplicationUrls:CustomerPortal") ?? "#";
                var reopenUrl = _configuration.GetValue<string>("ApplicationUrls:ReopenTicket") ?? "#";
                var organizationName = _configuration.GetValue<string>("EmailSettings:OrganizationName") ?? "Support Team";

                var customer = await _customerService.GetCustomerById(ticket.CustomerId.Value); // ← Use .Value here
                if (customer == null)
                {
                    _logger.LogWarning($"Customer not found for ticket {ticketId}");
                    return;
                }

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
                    updatedByAgent: agentName,
                    agentNote: agentNote,
                    portalUrl: portalUrl,
                    reopenUrl: reopenUrl
                    );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error notifying ticket status change for ticket {ticketId}");
            }
        }

        public async Task NotifyTicketRepliedAsync(int ticketId, string agentName, string agentRole)
        {
            try
            {
                var ticketResult = await _ticketQueries.GetTicket(ticketId);
                if (ticketResult.IsFailure)
                {
                    _logger.LogWarning($"Could not find ticket {ticketId} to send reply email notification");
                    return;
                }

                var ticket = ticketResult.Value;

                if (!ticket.CustomerId.HasValue)
                {
                    _logger.LogWarning($"Ticket {ticketId} has no customer ID, skipping email notification");
                    return;
                }

                var portalUrl = _configuration.GetValue<string>("ApplicationUrls:CustomerPortal") ?? "#";
                var replyUrl = _configuration.GetValue<string>("ApplicationUrls:ReplyToTicket") ?? "#";
                var viewHistoryUrl = _configuration.GetValue<string>("ApplicationUrls:ViewTicketHistory") ?? "#";
                var reopenUrl = _configuration.GetValue<string>("ApplicationUrls:ReopenTicket") ?? "#";
                var organizationName = _configuration.GetValue<string>("EmailSettings:OrganizationName") ?? "Support Team";

                var customer = await _customerService.GetCustomerById(ticket.CustomerId.Value);
                if (customer == null)
                {
                    _logger.LogWarning($"Customer not found for ticket {ticketId}");
                    return;
                }

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
                    organizationName: organizationName,
                    recipientName: customer.Name ?? "Customer",
                    agentName: agentName,
                    agentRole: agentRole,
                    ticketStatus: ticket.Status.ToString(),
                    ticketStatusLabel: ticket.Status.ToString(),
                    replyUrl: replyUrl,
                    portalUrl: portalUrl,
                    viewHistoryUrl: viewHistoryUrl,
                    reopenUrl: reopenUrl
                    );

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error notifying ticket reply for ticket {ticketId}");
            }
        }
    }
}
