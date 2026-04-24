using CCP.Shared.ValueObjects;
using CustomerService.Sdk.Models;
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
            EmailSent email, int ticketId, TicketStatus ticketStatus,
            string organizationName, string expectedResponseTime,
            string portalUrl)
        {
            var component = await RenderComponentAsync<TicketCreatedEmail>(p =>
            {
                p.Add(nameof(TicketCreatedEmail.Email), email);
                p.Add(nameof(TicketCreatedEmail.TicketId), ticketId);
                p.Add(nameof(TicketCreatedEmail.TicketStatus), ticketStatus);
                p.Add(nameof(TicketCreatedEmail.OrganizationName), organizationName);
                p.Add(nameof(TicketCreatedEmail.ExpectedResponseTime), expectedResponseTime);
                p.Add(nameof(TicketCreatedEmail.PortalUrl), portalUrl);
            });


            return PreMailer.Net.PreMailer.MoveCssInline(component).Html;
        }

        public async Task<string> RenderTicketReplyEmailAsync(
            EmailReceived email, int ticketId, TicketStatus ticketStatus,
            CustomerDTO customer, string organizationName,
            string agentName, string agentRole,
            string replyUrl, string viewHistoryUrl)
        {
            var component = await RenderComponentAsync<TicketReplyEmail>(p =>
            {
                p.Add(nameof(TicketReplyEmail.Email), email);
                p.Add(nameof(TicketReplyEmail.TicketId), ticketId);
                p.Add(nameof(TicketReplyEmail.TicketStatus), ticketStatus);
                p.Add(nameof(TicketReplyEmail.Customer), customer);
                p.Add(nameof(TicketReplyEmail.OrganizationName), organizationName);
                p.Add(nameof(TicketReplyEmail.AgentName), agentName);
                p.Add(nameof(TicketReplyEmail.AgentRole), agentRole);
                p.Add(nameof(TicketReplyEmail.ReplyUrl), replyUrl);
                p.Add(nameof(TicketReplyEmail.ViewHistoryUrl), viewHistoryUrl);
            });

            return PreMailer.Net.PreMailer.MoveCssInline(component).Html;
        }



        public async Task<string> RenderTicketStatusEmailAsync(
            EmailSent email, int ticketId, TicketStatus ticketStatus,
            string organizationName, string oldStatusLabel,
            string portalUrl)
        {
            var component = await RenderComponentAsync<TicketStatusEmail>(p =>
            {
                p.Add(nameof(TicketStatusEmail.Email), email);
                p.Add(nameof(TicketStatusEmail.TicketId), ticketId);
                p.Add(nameof(TicketStatusEmail.OrganizationName), organizationName);
                p.Add(nameof(TicketStatusEmail.OldStatusLabel), oldStatusLabel);
                p.Add(nameof(TicketStatusEmail.PortalUrl), portalUrl);
            });

            return PreMailer.Net.PreMailer.MoveCssInline(component).Html;
        }

        public async Task<string> RenderReplyToEmailAsync(
            EmailReceived emailReceived, EmailSent? emailSent,
            int ticketId, string organizationName)
        {
            var component = await RenderComponentAsync<ReplyToEmail>(p =>
            {
                p.Add(nameof(ReplyToEmail.ReceivedEmail), emailReceived);
                p.Add(nameof(ReplyToEmail.SentEmail), emailSent);
                p.Add(nameof(ReplyToEmail.TicketId), ticketId);
                p.Add(nameof(ReplyToEmail.OrganizationName), organizationName);
            });
            return PreMailer.Net.PreMailer.MoveCssInline(component).Html;
        }

        public Task<string> RenderSupportCustomerReplyNotificationAsync(
            EmailReceived email, int ticketId, TicketStatus ticketStatus,
            CustomerDTO customer, string organizationName,
            string replyUrl, string mangmentUrl,
            string viewHistoryUrl) =>
            RenderComponentAsync<SupportCustomerReplyNotification>(p =>
            {
                p.Add(nameof(SupportCustomerReplyNotification.Email), email);
                p.Add(nameof(SupportCustomerReplyNotification.TicketId), ticketId);
                p.Add(nameof(SupportCustomerReplyNotification.TicketStatus), ticketStatus);
                p.Add(nameof(SupportCustomerReplyNotification.Customer), customer);
                p.Add(nameof(SupportCustomerReplyNotification.OrganizationName), organizationName);
                p.Add(nameof(SupportCustomerReplyNotification.ReplyUrl), replyUrl);
                p.Add(nameof(SupportCustomerReplyNotification.ManagementUrl), mangmentUrl);
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
