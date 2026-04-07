using System;
using System.Collections.Generic;
using System.Text;
using CCP.Shared.ResultAbstraction;
using EmailService.Domain.Models;

namespace EmailService.Domain.Interfaces
{
    public interface IEmailReceived
    {
        Task<EmailReceived?> GetByIdAsync(int id);
        Task<EmailReceived?> GetByOrganizationIdAsync(Guid organizationId);
        Task<Result> CreateAsync(EmailReceived email);
        Task<Result> DeleteAsync(int id);
    }
}
