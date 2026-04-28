using System;
using System.Collections.Generic;
using System.Text;
using CCP.Shared.ResultAbstraction;
using EmailService.API.Services;
using EmailService.Domain.Interfaces;
using EmailService.Domain.Models;
using IdentityService.Application.Services.Customer;
using IdentityService.Application.Services.Organization;
using IdentityService.Application.Services.User;
using Keycloak.Sdk.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using TicketService.Domain.Entities;

namespace EmailService.NotifcationService.Infrastructure.Services
{
    public class TicketNotificationService : ITicketNotificationService
    {
        private readonly IEmailSent _emailSentRepo;
        private readonly IEmailSender _emailSender;
        private readonly IEmailTemplateRenderer _emailTemplateRenderer;
        private readonly IUserService _userService;
        private readonly IOrganizationService _organizationService;

        public TicketNotificationService(
            IEmailSent emailSentRepo,
            IEmailSender emailSender,
            IEmailTemplateRenderer emailTemplateRenderer,
            IUserService userService,
            IOrganizationService organizationService
            )
        {
            _emailSentRepo = emailSentRepo;
            _emailSender = emailSender;
            _emailTemplateRenderer = emailTemplateRenderer;
            _userService = userService;
            _organizationService = organizationService;
        }

        public async Task<Result> SendTicketCreatedNotificationsAsync(
            Ticket ticket,
            Guid? assignedUserId,
            string tenantEmail,
            CancellationToken ct = default)
        {
            if (!ticket.CustomerId.HasValue)
            {
                return Result.Failure(Error.Failure("CustomerIdMissing", "Ticket customer id is required."));
            }

            var customerResult = await _userService.GetUserDetails(ticket.CustomerId.Value, ct);

            if (customerResult.IsFailure || customerResult.Value is null || string.IsNullOrWhiteSpace(customerResult.Value.Email))
            {
                return Result.Failure(
                    customerResult.IsFailure
                        ? customerResult.Error
                        : Error.NotFound("Customer", "Customer email not found."));
            }



            var customerEmailModel = new EmailSent
            {
                SenderAddress = tenantEmail,
                OrganizationId = ticket.OrganizationId,
                Subject = ticket.Title,
                RecipientAddress = customerResult.Value.Email,
                Body = ticket.Description ?? "",
                SentAt = DateTime.UtcNow,
            };

            var organizationNameResult = await _organizationService.GetOrganizationNameById(ticket.OrganizationId, ct);
            if (organizationNameResult.IsFailure || string.IsNullOrWhiteSpace(organizationNameResult.Value))
            {
                return Result.Failure(organizationNameResult.Error);
            }

            var customerHtml = await _emailTemplateRenderer.RenderTicketCreatedEmailAsync(
                customerEmailModel,
                organizationNameResult.Value,
                "24 hours",
                "google.com" // fix to actual url
                );

            await _emailSender.SendEmailAsync(
                customerResult.Value.Email,
                ticket.Title,
                customerHtml
                );

            await _emailSentRepo.CreateAsync(customerEmailModel);

            UserKeycloakAccount? supporter = null;

            if (assignedUserId is Guid supporterId)
            {
                var supporterResult = await _userService.GetUserDetails(supporterId, ct);

                if (supporterResult.IsSuccess && supporterResult.Value is not null && !string.IsNullOrWhiteSpace(supporterResult.Value.Email))
                {
                    supporter = supporterResult.Value;
                }
            }

            var supporterEmailModel = new EmailSent
            {
                SenderAddress = tenantEmail,
                OrganizationId = ticket.OrganizationId,
                Subject = ticket.Title,
                RecipientAddress = supporter?.Email ?? "",
                Body = ticket.Description ?? "",
                SentAt = DateTime.UtcNow,
            };

            if (supporter == null)
                return Result.Success();

            var supportHtml = await _emailTemplateRenderer.RenderSupportTicketNotificationAsync(
                supporterEmailModel,
                supporter?.Email ?? "",
                organizationNameResult.Value,
                "24 hours",
                "google.com" // fix to actual url
            );

            if (supporter is { Email: { } supporterEmail } && !string.IsNullOrWhiteSpace(supporterEmail))
            {
                await _emailSender.SendEmailAsync(
                    supporterEmail,
                    $"New ticket: {ticket.Title}",
                    supportHtml
                );

                await _emailSentRepo.CreateAsync(supporterEmailModel);
            }
            return Result.Success();

        }
    }
}
