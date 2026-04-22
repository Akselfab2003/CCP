using CustomerService.Sdk.Models;
using EmailService.Application.Interfaces;
using EmailService.Domain.Models;
using Microsoft.Extensions.Logging;
using TicketService.Sdk.Dtos;

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
            string recipientEmail,string ticketTitle,
            EmailSent emailModel, TicketSdkDto ticket,
            string organizationName, string expectedResponseTime,
            string portalUrl)
        {
            try
            {
                await _emailSendingService.SendTicketCreatedEmailAsync(
                    to: recipientEmail,
                    subject: $"[New Ticket] {ticketTitle}",
                    email: emailModel,
                    ticket: ticket,
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
            string recipientEmail, string ticketTitle,
            EmailReceived emailModel, TicketSdkDto ticket,
            CustomerDTO customer, string organizationName,
            string agentName,string agentRole,
            string replyUrl,string viewHistoryUrl)
        {
            try
            {
                await _emailSendingService.SendTicketReplyEmailAsync(
                    to: recipientEmail,
                    subject: $"[Reply] {ticketTitle}",
                    email: emailModel,
                    ticket: ticket,
                    customer: customer,
                    organizationName: organizationName,
                    agentName: agentName,
                    agentRole: agentRole,
                    replyUrl: replyUrl,
                    viewHistoryUrl: viewHistoryUrl
);

                _logger.LogInformation($"Ticket reply email sent to {recipientEmail} for ticket: {ticketTitle}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send ticket reply email to {recipientEmail} for ticket: {ticketTitle}");
            }
        }

        public async Task SendTicketStatusChangeNotificationAsync(
            string recipientEmail,string ticketTitle,
            EmailSent emailModel, TicketSdkDto ticket,
            string organizationName, string oldStatusLabel,
            string portalUrl)
        {
            try
            {
                await _emailSendingService.SendTicketStatusEmailAsync(
                    to: recipientEmail,
                    subject: $"[Status Update] {ticketTitle} - Now {ticket.Status}",
                    email: emailModel,
                    ticket : ticket,
                    organizationName: organizationName,
                    oldStatusLabel: oldStatusLabel,
                    portalUrl: portalUrl);

                _logger.LogInformation($"Ticket status change email sent to {recipientEmail} for ticket: {ticketTitle}. Status: {ticket.Status}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send ticket status change email to {recipientEmail} for ticket: {ticketTitle}");
            }
        }
        public async Task SendSupportCustomerReplyNotificationAsync(
            string recipientEmail,EmailReceived emailModel,
            TicketSdkDto ticket, CustomerDTO customer,
            string organizationName, string replyUrl,
            string mangmentUrl, string viewHistoryUrl)
        {
            try
            {
                await _emailSendingService.SendSupportCustomerReplyEmailAsync(
                    to: recipientEmail,
                    subject: $"[Customer Reply] {customer.Name} has replied to their support ticket",
                    email: emailModel,
                    ticket : ticket,
                    customer: customer,
                    organizationName: organizationName,
                    replyUrl: replyUrl,
                    mangmentUrl: mangmentUrl,
                    viewHistoryUrl: viewHistoryUrl
                    );

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
            string recipientEmail,EmailReceived emailReceived,
            EmailSent emailSent, TicketSdkDto ticket,
            string organizationName)
        {
            try
            {
                await _emailSendingService.SendReplyToEmailAsync(
                    to: recipientEmail,
                    subject: $"[New Reply] Ticket #{ticket.Id}",
                    emailReceived: emailReceived,
                    emailSent: emailSent,
                    ticket: ticket,
                    organizationName: organizationName);

                _logger.LogInformation("Reply-to email sent to {Recipient} for ticket #{TicketId}",
                    recipientEmail, ticket.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send reply-to email to {Recipient} for ticket #{TicketId}",
                    recipientEmail, ticket.Id);
            }
        }
    }
}
