using CCP.Shared.ResultAbstraction;
using EmailService.Domain.Models;

namespace EmailService.Domain.Interfaces
{
    public interface ITenantEmailConfigurationRepo
    {
        Task<Result> AddAsync(TenantEmailConfiguration tenantEmailConfiguration);
        Task<Result> DeleteAsync(int id);
        Task<Result<TenantEmailConfiguration>> GetByIdAsync(Guid id);
        Task<Result> UpdateAsync(TenantEmailConfiguration tenantEmailConfiguration);
    }
}