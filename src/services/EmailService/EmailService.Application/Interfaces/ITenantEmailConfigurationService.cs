using CCP.Shared.ResultAbstraction;
using EmailService.Domain.Requests;

namespace EmailService.Application.Interfaces
{
    public interface ITenantEmailConfigurationService
    {
        Task<Result> AddTenantEmailConfigurationAsync(AddTenantEmailConfigurationRequest request);
    }
}