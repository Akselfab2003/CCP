using CCP.Shared.ResultAbstraction;
using EmailService.Domain.Models;
using MimeKit;

namespace EmailService.Worker.Host.Services
{
    public interface IMailManagementController
    {
        Task<Result<string>> GetCustomerEmailFromMail(MimeMessage message, TenantEmailConfiguration tenantEmailConfiguration);
    }
}