using CCP.Shared.ResultAbstraction;
using IdentityService.Sdk.Models;

namespace IdentityService.Sdk.Services.Tenant
{
    public interface ITenantService
    {
        Task<Result> CreateTenant(CreateTenantDTO createTenant, CancellationToken ct = default);
        Task<Result<List<TenantMember>>> GetAllTenantMemberAsync(CancellationToken ct = default);
        Task<Result> InviteNewTenantMember(string email, CancellationToken ct = default);
    }
}
