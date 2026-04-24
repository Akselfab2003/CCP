using CCP.Shared.ResultAbstraction;

namespace EmailService.Application.Interfaces
{
    public interface ITenantEmailConfigurationService
    {
        Task<Result> AddTenantEmailConfigurationAsync(string DefaultSenderEmail);
    }
}
