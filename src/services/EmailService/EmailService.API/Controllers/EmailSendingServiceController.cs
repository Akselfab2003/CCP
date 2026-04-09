using EmailService.Api.Services;
using EmailService.Application.Interfaces;
using EmailService.Domain.Interfaces;
using EmailService.Domain.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;



namespace EmailService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailSendingServiceController : ControllerBase
    {
        private readonly IEmail _emailLogic;
        private readonly EmailTemplateRenderer _emailTemplateRenderer;
        private readonly IEmailSent _emailSentRepo;

        public EmailSendingServiceController(IEmail emailLogic, EmailTemplateRenderer emailTemplateRenderer, IEmailSent emailSentRepo)
        {
            _emailLogic = emailLogic;
            _emailTemplateRenderer = emailTemplateRenderer;
            _emailSentRepo = emailSentRepo;
        }
        [HttpPost("send-ticket-created")]
        public async Task<ActionResult> SendTicketCreatedEmail(TicketCreatedRequest request)
        {
            try
            {
                var orgName = request.OrganizationName ?? "Support";
                var responseTime = request.ExpectedResponseTime ?? "24 hours";
                var portalUrl = request.PortalUrl ?? "#";
                var recipientName = request.RecipientName ?? request.Email.RecipientAddress;

                try { await _emailSentRepo.CreateAsync(request.Email); } catch { }

                var customerHtml = await _emailTemplateRenderer.RenderTicketCreatedEmailAsync(
                   request.Email, orgName, responseTime, portalUrl);

                try
                {
                    await _emailLogic.SendHtmlEmail(
                        request.Email.SenderAddress, orgName,
                        request.Email.RecipientAddress, recipientName,
                        request.Email.Subject, customerHtml);
                }
                catch { }

                if (!string.IsNullOrEmpty(request.SupportTeamEmail))
                {
                    var supportHtml = await _emailTemplateRenderer.RenderSupportTicketNotificationAsync(
                        request.Email, recipientName, orgName, responseTime, request.ManagementUrl ?? "#");

                    try
                    {
                        await _emailLogic.SendHtmlEmail(
                            request.Email.SenderAddress, orgName,
                            request.SupportTeamEmail, "Support Team",
                            $"[New Ticket] {request.Email.Subject}", supportHtml);
                    }
                    catch { }
                }

                return Ok("Ticket created email sent successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"{ex.GetType().Name}: {ex.Message} | Inner: {ex.InnerException?.Message}");
            }
        }

        [HttpPost("send-ticket-reply")]
        public async Task<ActionResult> SendTicketReplyEmail(TicketReplyRequest request)
        {
            var orgName = request.OrganizationName ?? "Support";
            var ticketStatus = request.TicketStatus ?? "open";
            var ticketStatusLabel = request.TicketStatusLabel ?? "Open";
            var replyUrl = request.ReplyUrl ?? "#";
            var viewHistoryUrl = request.ViewHistoryUrl ?? "#";
            var recipientName = request.RecipientName ?? request.Email.RecipientAddress;

            if (request.IsCustomerReply)
            {
                if (!string.IsNullOrEmpty(request.SupportTeamEmail))
                {
                    var supportHtml = await _emailTemplateRenderer.RenderSupportCustomerReplyNotificationAsync(
                        request.Email, recipientName, request.Email.SenderAddress,
                        orgName, ticketStatus, ticketStatusLabel,
                        replyUrl, request.ManagementUrl ?? "#", viewHistoryUrl);

                    await _emailLogic.SendHtmlEmail(
                        request.Email.SenderAddress, orgName,
                        request.SupportTeamEmail, "Support Team",
                        $"[Customer Reply] {request.Email.Subject}", supportHtml);
                }
            }
            else
            {
                var customerHtml = await _emailTemplateRenderer.RenderTicketReplyEmailAsync(
                    request.Email, recipientName, orgName,
                    request.AgentName ?? "Support Team", request.AgentRole ?? "Support Agent",
                    ticketStatus, ticketStatusLabel,
                    replyUrl, request.PortalUrl ?? "#", viewHistoryUrl, request.ReopenUrl ?? "#");

                await _emailLogic.SendHtmlEmail(
                    request.Email.SenderAddress, orgName,
                    request.Email.RecipientAddress, recipientName,
                    request.Email.Subject, customerHtml);
            }

            return Ok("Ticket reply email sent successfully.");
        }

        [HttpPost("send-ticket-status")]
        public async Task<ActionResult> SendTicketStatusEmail(TicketStatusRequest request)
        {
            var orgName = request.OrganizationName ?? "Support";
            var recipientName = request.RecipientName ?? request.Email.RecipientAddress;

            var html = await _emailTemplateRenderer.RenderTicketStatusEmailAsync(
                request.Email, orgName,
                request.NewStatus ?? "resolved", request.NewStatusLabel ?? "Resolved",
                request.OldStatusLabel ?? "Open", request.UpdatedByAgent ?? "",
                request.AgentNote ?? "", request.PortalUrl ?? "#", request.ReopenUrl ?? "#");

            await _emailLogic.SendHtmlEmail(
                request.Email.SenderAddress, orgName,
                request.Email.RecipientAddress, recipientName,
                request.Email.Subject, html);

            return Ok("Ticket status email sent successfully.");
        }

    }
}
