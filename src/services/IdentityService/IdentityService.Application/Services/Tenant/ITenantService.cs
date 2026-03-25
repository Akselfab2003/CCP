using CCP.Shared.ResultAbstraction;
using IdentityService.Application.Models;

namespace IdentityService.Application.Services.Tenant
{
    public interface ITenantService
    {
        Task<Result> CreateTenant(CreateTenantRequest request);
    }
}
