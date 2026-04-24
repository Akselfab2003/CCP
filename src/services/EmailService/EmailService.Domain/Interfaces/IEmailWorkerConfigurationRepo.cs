using CCP.Shared.ResultAbstraction;
using EmailService.Domain.Models;

namespace EmailService.Domain.Interfaces
{
    public interface IEmailWorkerConfigurationRepo
    {
        Task<Result<List<TenantEmailConfiguration>>> GetAllAsync();
        Task<Result<TenantEmailConfiguration>> GetByIdAsync(Guid id);
    }
}
