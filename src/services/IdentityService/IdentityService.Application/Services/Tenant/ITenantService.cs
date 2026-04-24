using CCP.Shared.ResultAbstraction;
using IdentityService.API.Endpoints;
using IdentityService.Application.Models;

namespace IdentityService.Application.Services.Tenant
{
    public interface ITenantService
    {
        Task<Result> CreateTenant(CreateTenantRequest request);
        Task<Result<TenantInfoDto>> GetTenantDetails(Guid? tenantId, string? Domain);
    }
}
