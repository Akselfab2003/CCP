using CCP.Shared.Events;
using CCP.Shared.ResultAbstraction;
using EmailService.Domain.Models;
using MimeKit;

namespace EmailService.Worker.Host.Services
{
    public interface IMailBoxService
    {
        Task<Result<MimeMessage>> GetMailFromMailServer(string MessageId, TenantEmailConfiguration tenantEmailConfiguration);
        Task<Result<TenantEmailConfiguration>> GetTenantMailboxDetails(mail_received mail_Received);
    }
}
