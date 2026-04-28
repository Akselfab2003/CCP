using System;
using System.Collections.Generic;
using System.Text;
using CCP.Shared.ResultAbstraction;
using TicketService.Domain.Entities;

namespace EmailService.NotifcationService.Infrastructure.Services
{
    public interface ITicketNotificationService
    {
        Task<Result> SendTicketCreatedNotificationsAsync(
            Ticket ticket,
            Guid? assignedUserId,
            string tenantEmail,
            CancellationToken ct = default);
    }
}
