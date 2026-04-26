using CCP.Shared.ValueObjects;
using CustomerService.Sdk.Models;
using EmailService.Application.Interfaces;
using EmailService.Domain.Interfaces;
using EmailService.Domain.Models;
using EmailTemplates.Renderes;
using MessagingService.Sdk.Dtos;
using MimeKit;
using MimeKit.Utils;

namespace EmailService.Infrastructure.EmailInfrastructure
{
    public class EmailSendingService : IEmail
    {
        private readonly ISmtpClient _smtpClient;
        private readonly IEmailTemplateRenderer _emailTemplateRenderer;
        private readonly IEmailTicketMessageRepository _emailTicketMessageRepository;
        private readonly ITenantEmailConfigurationRepo _tenantEmailConfigurationRepo;

        public EmailSendingService(ISmtpClient smtpClient, IEmailTemplateRenderer emailTemplateRenderer,ITenantEmailConfigurationRepo tenantEmailConfigurationRepo, IEmailTicketMessageRepository emailTicketMessageRepository)
        {
            _smtpClient = smtpClient;
            _emailTemplateRenderer = emailTemplateRenderer;
            _emailTicketMessageRepository = emailTicketMessageRepository;
            _tenantEmailConfigurationRepo = tenantEmailConfigurationRepo;
        }

        public async Task SendTicketCreatedEmailAsync(
            string to, string subject,
            EmailSent email, int ticketId, TicketStatus ticketStatus,
            string organizationName, string expectedResponseTime,
            string portalUrl, TicketOrigin origin)
        {
            var tenant = await _tenantEmailConfigurationRepo.GetByTenantIdAsync(email.OrganizationId);
            if (tenant.IsSuccess)
            {
                string fromaddress = tenant.Value.DefaultSenderEmail;

                email.SenderAddress = fromaddress;
            }

            var htmlContent = await _emailTemplateRenderer
                .RenderTicketCreatedEmailAsync(
                email, ticketId, ticketStatus,
                organizationName, expectedResponseTime,
                portalUrl);

            var message = await BuildMessage(
                fromAddress: email.SenderAddress,
                fromName: organizationName,
                toAddress: to,
                toName: email.RecipientAddress,
                subject: subject,
                origin: origin,
                ticketId: ticketId
            );

            message.Body = new BodyBuilder { HtmlBody = htmlContent }.ToMessageBody();

            await _smtpClient.SendAsync(message);
        }

        public async Task SendTicketReplyEmailAsync(
            string to, string subject,
            EmailReceived email, int ticketId, TicketStatus ticketStatus,

            CustomerDTO customer, string organizationName,
            string agentName, string agentRole,
            string replyUrl, string viewHistoryUrl, TicketOrigin origin)
        {
            var tenant = await _tenantEmailConfigurationRepo.GetByTenantIdAsync(email.OrganizationId);
            if (tenant.IsSuccess)
            {
                string fromaddress = tenant.Value.DefaultSenderEmail;

                email.SenderAddress = fromaddress;
            }

            var htmlContent = await _emailTemplateRenderer
                .RenderTicketReplyEmailAsync(
                email, ticketId, ticketStatus,
                customer, organizationName,
                agentName, agentRole,
                replyUrl, viewHistoryUrl);

            var message = await BuildMessage(
                fromAddress: email.SenderAddress,
                fromName: organizationName,
                toAddress: to,
                toName: email.RecipientAddress,
                subject: subject,
                origin: origin,
                ticketId: ticketId
            );
            message.Body = new BodyBuilder { HtmlBody = htmlContent }.ToMessageBody();
            await _smtpClient.SendAsync(message);
        }

        public async Task SendTicketStatusEmailAsync(
            string to, string subject,
            EmailSent email, int ticketId, TicketStatus ticketStatus,
            string organizationName, string oldStatusLabel,
            string portalUrl, TicketOrigin origin)
        {
            var tenant = await _tenantEmailConfigurationRepo.GetByTenantIdAsync(email.OrganizationId);
            if (tenant.IsSuccess)
            {
                string fromaddress = tenant.Value.DefaultSenderEmail;

                email.SenderAddress = fromaddress;
            }

            var htmlContent = await _emailTemplateRenderer
                .RenderTicketStatusEmailAsync(
                email, ticketId, ticketStatus,
                organizationName, oldStatusLabel,
                portalUrl);

            var message = await BuildMessage(
                fromAddress: email.SenderAddress,
                fromName: organizationName,
                toAddress: to,
                toName: email.RecipientAddress,
                subject: subject,
                origin: origin,
                ticketId: ticketId
            );
            message.Body = new BodyBuilder { HtmlBody = htmlContent }.ToMessageBody();
            await _smtpClient.SendAsync(message);
        }

        public async Task SendSupportCustomerReplyEmailAsync(
            string to, string subject,
            EmailReceived email, int ticketId, TicketStatus ticketStatus,
            CustomerDTO customer, string organizationName,
            string replyUrl, string managementUrl,
            string viewHistoryUrl, TicketOrigin origin)
        {
            var tenant = await _tenantEmailConfigurationRepo.GetByTenantIdAsync(email.OrganizationId);
            if (tenant.IsSuccess)
            {
                string fromaddress = tenant.Value.DefaultSenderEmail;

                email.SenderAddress = fromaddress;
            }

            var htmlContent = await _emailTemplateRenderer
                .RenderSupportCustomerReplyNotificationAsync(
                email, ticketId, ticketStatus,
                customer, organizationName,
                replyUrl, managementUrl,
                viewHistoryUrl);

            var message = await BuildMessage(
                fromAddress: email.SenderAddress,
                fromName: organizationName,
                toAddress: to,
                toName: email.RecipientAddress,
                subject: subject,
                origin: origin,
                ticketId: ticketId
            );
            message.Body = new BodyBuilder { HtmlBody = htmlContent }.ToMessageBody();
            await _smtpClient.SendAsync(message);
        }

        public async Task SendReplyToEmailAsync(
            string to, string subject,
            List<MessageDto> messages, EmailSent emailSent,
            int ticketId, TicketStatus ticketStatus,
            string organizationName,TicketOrigin origin)
        {
            var tenant = await _tenantEmailConfigurationRepo.GetByTenantIdAsync(emailSent.OrganizationId);
            if (tenant.IsSuccess)
            {
                string fromaddress = tenant.Value.DefaultSenderEmail;

                emailSent.SenderAddress = fromaddress;
            }

            var htmlContent = await _emailTemplateRenderer
                .RenderReplyToEmailAsync(
                messages, emailSent,
                ticketId, organizationName);

            var message = await BuildMessage(
                fromAddress: emailSent.SenderAddress,
                fromName: organizationName,
                toAddress: to,
                toName: emailSent.RecipientAddress,
                subject: subject,
                origin: origin,
                ticketId: ticketId
            );
            message.Body = new BodyBuilder { HtmlBody = htmlContent }.ToMessageBody();
            await _smtpClient.SendAsync(message);
        }


        private async Task<MimeMessage> BuildMessage(
        string fromAddress, string fromName,
        string toAddress, string toName,
        string subject, TicketOrigin origin, int ticketId)
        {
            var message = new MimeMessage();
            if (origin == TicketOrigin.Email)
            {
                message = await GetLastEmail(ticketId: ticketId, message);
            }

            message.From.Add(new MailboxAddress(fromName, fromAddress));
            message.To.Add(new MailboxAddress(toName, toAddress));
            message.Subject = subject;
            return message;

        }

        private async Task<MimeMessage> GetLastEmail(int ticketId, MimeMessage message)
        {
            var allEmailsFromTicket = await _emailTicketMessageRepository.GetByTicketIdAsync(ticketId);
            if (allEmailsFromTicket.IsFailure) { return message; }

            var lastEmail = allEmailsFromTicket.Value.OrderByDescending(e => e.SentAt).FirstOrDefault();
            if (lastEmail == null) { return message; }

                message.InReplyTo = lastEmail.MessageId;
                message.MessageId = MimeUtils.GenerateMessageId();
                message.References.AddRange(lastEmail.References);
                message.References.Add(lastEmail.MessageId);

            return message;
        }

    }
}
