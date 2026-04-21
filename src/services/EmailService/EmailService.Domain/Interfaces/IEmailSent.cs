using System;
using System.Collections.Generic;
using System.Text;
using CCP.Shared.ResultAbstraction;
using EmailService.Domain.Models;

namespace EmailService.Domain.Interfaces
{
    public interface IEmailSent
    {
        Task<EmailSent?> GetByIdAsync(int id);
        Task<EmailSent?> GetByTicketIdAsync(int ticketId);
        Task<EmailSent?> GetByOrganizationIdAsync(Guid organizationId);
        Task<Result> CreateAsync(EmailSent email);
        Task<Result> DeleteAsync(int id);
    }
}
