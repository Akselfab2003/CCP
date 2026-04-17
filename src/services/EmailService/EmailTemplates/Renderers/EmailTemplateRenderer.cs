using EmailService.Domain.Models;
using EmailTemplates.EmailTemplates;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
namespace EmailTemplates.Renderes
{
    public class EmailTemplateRenderer : IEmailTemplateRenderer
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILoggerFactory _loggerFactory;

        public EmailTemplateRenderer(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            _serviceProvider = serviceProvider;
            _loggerFactory = loggerFactory;
        }

        public async Task<string> RenderTicketCreatedEmailAsync(
            EmailSent email, string organizationName, string expectedResponseTime, string portalUrl)
        {
            var component = await RenderComponentAsync<TicketCreatedEmail>(p =>
            {
                p.Add(nameof(TicketCreatedEmail.Email), email);
                p.Add(nameof(TicketCreatedEmail.OrganizationName), organizationName);
                p.Add(nameof(TicketCreatedEmail.ExpectedResponseTime), expectedResponseTime);
                p.Add(nameof(TicketCreatedEmail.PortalUrl), portalUrl);
            });

            
            return PreMailer.Net.PreMailer.MoveCssInline(component).Html;
        }

        public async Task<string> RenderTicketReplyEmailAsync(
            EmailReceived email, string recipientName, string organizationName,
            string agentName, string agentRole, string ticketStatus, string ticketStatusLabel,
            string replyUrl, string portalUrl, string viewHistoryUrl, string reopenUrl)
        {
            var component = await RenderComponentAsync<TicketReplyEmail>(p =>
            {
                p.Add(nameof(TicketReplyEmail.Email), email);
                p.Add(nameof(TicketReplyEmail.RecipientName), recipientName);
                p.Add(nameof(TicketReplyEmail.OrganizationName), organizationName);
                p.Add(nameof(TicketReplyEmail.AgentName), agentName);
                p.Add(nameof(TicketReplyEmail.AgentRole), agentRole);
                p.Add(nameof(TicketReplyEmail.TicketStatus), ticketStatus);
                p.Add(nameof(TicketReplyEmail.TicketStatusLabel), ticketStatusLabel);
                p.Add(nameof(TicketReplyEmail.ReplyUrl), replyUrl);
                p.Add(nameof(TicketReplyEmail.PortalUrl), portalUrl);
                p.Add(nameof(TicketReplyEmail.ViewHistoryUrl), viewHistoryUrl);
                p.Add(nameof(TicketReplyEmail.ReopenUrl), reopenUrl);
            });

            return PreMailer.Net.PreMailer.MoveCssInline(component).Html;
        }



        public async Task<string> RenderTicketStatusEmailAsync(
            EmailSent email, string organizationName, string newStatus, string newStatusLabel,
            string oldStatusLabel, string updatedByAgent, string agentNote,
            string portalUrl, string reopenUrl)
        {
                var component = await RenderComponentAsync<TicketStatusEmail>(p =>
                {
                    p.Add(nameof(TicketStatusEmail.Email), email);
                    p.Add(nameof(TicketStatusEmail.OrganizationName), organizationName);
                    p.Add(nameof(TicketStatusEmail.NewStatus), newStatus);
                    p.Add(nameof(TicketStatusEmail.NewStatusLabel), newStatusLabel);
                    p.Add(nameof(TicketStatusEmail.OldStatusLabel), oldStatusLabel);
                    p.Add(nameof(TicketStatusEmail.UpdatedByAgent), updatedByAgent);
                    p.Add(nameof(TicketStatusEmail.AgentNote), agentNote);
                    p.Add(nameof(TicketStatusEmail.PortalUrl), portalUrl);
                    p.Add(nameof(TicketStatusEmail.ReopenUrl), reopenUrl);
                });
    
                return PreMailer.Net.PreMailer.MoveCssInline(component).Html;
        }

        public Task<string> RenderSupportTicketNotificationAsync(
            EmailSent email, string customerEmail, string organizationName,
            string expectedResponseTime, string managementUrl) =>
            RenderComponentAsync<SupportTicketNotification>(p =>
            {
                p.Add(nameof(SupportTicketNotification.Email), email);
                p.Add(nameof(SupportTicketNotification.CustomerEmail), customerEmail);
                p.Add(nameof(SupportTicketNotification.OrganizationName), organizationName);
                p.Add(nameof(SupportTicketNotification.ExpectedResponseTime), expectedResponseTime);
                p.Add(nameof(SupportTicketNotification.ManagementUrl), managementUrl);
            });

        public Task<string> RenderSupportCustomerReplyNotificationAsync(
            EmailReceived email, string customerName, string customerEmail,
            string organizationName, string ticketStatus, string ticketStatusLabel,
            string replyUrl, string managementUrl, string viewHistoryUrl) =>
            RenderComponentAsync<SupportCustomerReplyNotification>(p =>
            {
                p.Add(nameof(SupportCustomerReplyNotification.Email), email);
                p.Add(nameof(SupportCustomerReplyNotification.CustomerName), customerName);
                p.Add(nameof(SupportCustomerReplyNotification.CustomerEmail), customerEmail);
                p.Add(nameof(SupportCustomerReplyNotification.OrganizationName), organizationName);
                p.Add(nameof(SupportCustomerReplyNotification.TicketStatus), ticketStatus);
                p.Add(nameof(SupportCustomerReplyNotification.TicketStatusLabel), ticketStatusLabel);
                p.Add(nameof(SupportCustomerReplyNotification.ReplyUrl), replyUrl);
                p.Add(nameof(SupportCustomerReplyNotification.ManagementUrl), managementUrl);
                p.Add(nameof(SupportCustomerReplyNotification.ViewHistoryUrl), viewHistoryUrl);
            });

        private async Task<string> RenderComponentAsync<TComponent>(
            Action<Dictionary<string, object?>> configureParameters)
            where TComponent : IComponent
        {
            await using var htmlRenderer = new HtmlRenderer(_serviceProvider, _loggerFactory);

            return await htmlRenderer.Dispatcher.InvokeAsync(async () =>
            {
                var parameters = new Dictionary<string, object?>();
                configureParameters(parameters);
                var output = await htmlRenderer.RenderComponentAsync<TComponent>(
                    ParameterView.FromDictionary(parameters));
                return output.ToHtmlString();
            });
        }
    }
}
