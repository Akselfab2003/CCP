using CCP.Shared.ResultAbstraction;
using EmailService.Domain.Models;

namespace EmailService.Application.Interfaces
{
    public interface ITenantEmailConfigurationService
    {
        Task<Result> AddTenantEmailConfigurationAsync(string DefaultSenderEmail);
        Task<Result> UpdateTenantEmailConfigurationAsync(string DefaultSenderEmail);
        Task<Result<TenantEmailConfiguration>> GetTenantEmailConfigurationAsync();
    }
}
