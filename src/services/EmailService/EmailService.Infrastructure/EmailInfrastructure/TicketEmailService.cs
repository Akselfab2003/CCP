using System;
using System.Collections.Generic;
using System.Text;
using EmailService.Application.Interfaces;
using EmailService.Domain.Models;
using EmailTemplates.EmailTemplates;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.Logging;

namespace EmailService.Infrastructure.EmailInfrastructure
{
    public class TicketEmailService : ITicketEmailService
    {
        private readonly IEmail _emailSendingService;
        private readonly ILogger<TicketEmailService> _logger;

        public TicketEmailService(IEmail emailSendingService, ILogger<TicketEmailService> logger)
        {
            _emailSendingService = emailSendingService;
            _logger = logger;
        }

        public async Task SendTicketCreatedNotificationAsync(
            string recipientEmail,
            string ticketTitle,
            EmailSent emailModel,
            string organizationName,
            string expectedResponseTime,
            string portalUrl)
        {
            try
            {
                await _emailSendingService.SendTicketCreatedEmailAsync(
                    to: recipientEmail,
                    subject: $"[New Ticket] {ticketTitle}",
                    email: emailModel,
                    organizationName: organizationName,
                    expectedResponseTime: expectedResponseTime,
                    portalUrl: portalUrl);

                _logger.LogInformation($"Ticket created email sent to {recipientEmail} for ticket: {ticketTitle}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send ticket created email to {recipientEmail} for ticket: {ticketTitle}");
            }
        }

        public async Task SendTicketReplyNotificationAsync(
            string recipientEmail,
            string ticketTitle,
            EmailReceived emailModel,
            string recipientName,
            string organizationName,
            string agentName,
            string agentRole,
            string ticketStatus,
            string ticketStatusLabel,
            string replyUrl,
            string portalUrl,
            string viewHistoryUrl,
            string reopenUrl)
        {
            try
            {
                await _emailSendingService.SendTicketReplyEmailAsync(
                    to: recipientEmail,
                    subject: $"[Reply] {ticketTitle}",
                    email: emailModel,
                    organizationName: organizationName,
                    recipientName: recipientName,
                    agentName: agentName,
                    agentRole: agentRole,
                    ticketStatus: ticketStatus,
                    ticketStatusLabel: ticketStatusLabel,
                    replyUrl: replyUrl,
                    portalUrl: portalUrl,
                    viewHistoryUrl: viewHistoryUrl,
                    reopenUrl: reopenUrl);

                _logger.LogInformation($"Ticket reply email sent to {recipientEmail} for ticket: {ticketTitle}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send ticket reply email to {recipientEmail} for ticket: {ticketTitle}");
            }
        }

        public async Task SendTicketStatusChangeNotificationAsync(
            string recipientEmail,
            string ticketTitle,
            EmailSent emailModel,
            string organizationName,
            string newStatus,
            string newStatusLabel,
            string oldStatusLabel,
            string updatedByAgent,
            string agentNote,
            string portalUrl,
            string reopenUrl)
        {
            try
            {
                await _emailSendingService.SendTicketStatusEmailAsync(
                    to: recipientEmail,
                    subject: $"[Status Update] {ticketTitle} - Now {newStatusLabel}",
                    email: emailModel,
                    organizationName: organizationName,
                    newStatus: newStatus,
                    newStatusLabel: newStatusLabel,
                    oldStatusLabel: oldStatusLabel,
                    updatedByAgent: updatedByAgent,
                    agentNote: agentNote,
                    portalUrl: portalUrl,
                    reopenUrl: reopenUrl);

                _logger.LogInformation($"Ticket status change email sent to {recipientEmail} for ticket: {ticketTitle}. Status: {newStatusLabel}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send ticket status change email to {recipientEmail} for ticket: {ticketTitle}");
            }
        }
        public async Task SendSupportCustomerReplyNotificationAsync(
            string recipientEmail,
            EmailReceived emailModel,
            string customerName,
            string customerEmail,
            string organizationName,
            string ticketStatus,
            string ticketStatusLabel,
            string replyUrl,
            string managementUrl,
            string viewHistoryUrl)
        {
            try
            {
                await _emailSendingService.SendSupportCustomerReplyEmailAsync(
                    to: recipientEmail,
                    subject: $"[Customer Reply] {customerName} has replied to their support ticket",
                    email: emailModel,
                    customerName: customerName,
                    customerEmail: customerEmail,
                    organizationName: organizationName,
                    ticketStatus: ticketStatus,
                    ticketStatusLabel: ticketStatusLabel,
                    replyUrl: replyUrl,
                    managementUrl: managementUrl,
                    viewHistoryUrl: viewHistoryUrl);

                _logger.LogInformation("Support customer-reply notification sent to {Recipient} for ticket #{TicketId}",
                    recipientEmail, emailModel.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send support customer-reply notification to {Recipient} for ticket #{TicketId}",
                    recipientEmail, emailModel.Id);
            }
        }
    }
}
