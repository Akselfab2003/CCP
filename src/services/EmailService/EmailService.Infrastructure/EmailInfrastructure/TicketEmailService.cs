using CCP.Shared.ValueObjects;
using CustomerService.Sdk.Models;
using EmailService.Application.Interfaces;
using EmailService.Domain.Models;
using MessagingService.Sdk.Dtos;
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
            string recipientEmail, string ticketTitle,
            EmailSent emailModel, int ticketId,
            TicketStatus ticketStatus,
            string organizationName, string expectedResponseTime,
            string portalUrl, TicketOrigin origin,Guid OrgId)
        {
            try
            {
                await _emailSendingService.SendTicketCreatedEmailAsync(
                    to: recipientEmail,
                    subject: $"[New Ticket] {ticketTitle}",
                    email: emailModel,
                    ticketId: ticketId,
                    ticketStatus: ticketStatus,
                    organizationName: organizationName,
                    expectedResponseTime: expectedResponseTime,
                    portalUrl: portalUrl,
                    origin: origin,
                    OrgId: OrgId);

                _logger.LogInformation($"Ticket created email sent to {recipientEmail} for ticket: {ticketTitle}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send ticket created email to {recipientEmail} for ticket: {ticketTitle}");
            }
        }

        public async Task SendTicketReplyNotificationAsync(
            string recipientEmail, string ticketTitle,
            EmailReceived emailModel, int ticketId,
            TicketStatus ticketStatus,
            CustomerDTO customer, string organizationName,
            string agentName, string agentRole,
            string replyUrl, string viewHistoryUrl, TicketOrigin origin, Guid OrgId)
        {
            try
            {
                await _emailSendingService.SendTicketReplyEmailAsync(
                    to: recipientEmail,
                    subject: $"[Reply] {ticketTitle}",
                    email: emailModel,
                    ticketId: ticketId,
                    ticketStatus: ticketStatus,
                    customer: customer,
                    organizationName: organizationName,
                    agentName: agentName,
                    agentRole: agentRole,
                    replyUrl: replyUrl,
                    viewHistoryUrl: viewHistoryUrl,
                    origin: origin,
                    OrgId: OrgId
);

                _logger.LogInformation($"Ticket reply email sent to {recipientEmail} for ticket: {ticketTitle}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send ticket reply email to {recipientEmail} for ticket: {ticketTitle}");
            }
        }

        public async Task SendTicketStatusChangeNotificationAsync(
            string recipientEmail, string ticketTitle,
            EmailSent emailModel, int ticketId,
            TicketStatus ticketStatus,
            string organizationName, string oldStatusLabel,
            string portalUrl, TicketOrigin origin, Guid OrgId)
        {
            try
            {
                await _emailSendingService.SendTicketStatusEmailAsync(
                    to: recipientEmail,
                    subject: $"[Status Update] {ticketTitle} - Now {ticketStatus}",
                    email: emailModel,
                    ticketId: ticketId,
                    ticketStatus: ticketStatus,
                    organizationName: organizationName,
                    oldStatusLabel: oldStatusLabel,
                    portalUrl: portalUrl,
                    origin: origin,
                    OrgId: OrgId);

                _logger.LogInformation($"Ticket status change email sent to {recipientEmail} for ticket: {ticketTitle}. Status: {ticketStatus}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send ticket status change email to {recipientEmail} for ticket: {ticketTitle}");
            }
        }
        public async Task SendSupportCustomerReplyNotificationAsync(
            string recipientEmail, EmailReceived emailModel,
            int ticketId, TicketStatus ticketStatus,
            CustomerDTO customer, string organizationName,
            string replyUrl, string managementUrl,
            string viewHistoryUrl, TicketOrigin origin,Guid OrgId)
        {
            try
            {
                await _emailSendingService.SendSupportCustomerReplyEmailAsync(
                    to: recipientEmail,
                    subject: $"[Customer Reply] {customer.Name} has replied to their support ticket",
                    email: emailModel,
                    customer: customer,
                    ticketId: ticketId,
                    ticketStatus: ticketStatus,
                    organizationName: organizationName,
                    replyUrl: replyUrl,
                    managementUrl: managementUrl,
                    viewHistoryUrl: viewHistoryUrl,
                    origin: origin,
                    OrgId: OrgId);

                _logger.LogInformation("Support customer-reply notification sent to {Recipient} for ticket #{TicketId}",
                    recipientEmail, emailModel.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send support customer-reply notification to {Recipient} for ticket #{TicketId}",
                    recipientEmail, emailModel.Id);
            }
        }

        public async Task SendReplyToEmailAsync(
            string recipientEmail, EmailSent email,
            List<MessageDto> messages, int ticketId,
            Guid customerId, Guid OrgId,
            string organizationName, TicketOrigin origin,
            TicketStatus ticketStatus)
        {
            try
            {
                await _emailSendingService.SendReplyToEmailAsync(
                    to: recipientEmail,
                    subject: $"[Reply] Ticket #{ticketId}",
                    messages: messages,
                    emailSent: email,
                    ticketId: ticketId,
                    CustomerId: customerId,
                    OrgId: OrgId,
                    ticketStatus: ticketStatus,
                    organizationName: organizationName,
                    origin: origin);

                _logger.LogInformation("Reply-to email sent to {Recipient} for ticket #{TicketId}",
                    recipientEmail, ticketId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send reply-to email to {Recipient} for ticket #{TicketId}",
                    recipientEmail, ticketId);
            }
        }
    }
}
