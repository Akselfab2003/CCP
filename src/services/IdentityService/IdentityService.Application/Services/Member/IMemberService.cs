using CCP.Shared.ResultAbstraction;
using IdentityService.Application.Models;

namespace IdentityService.Application.Services.Member
{
    public interface IMemberService
    {
        Task<Result<List<TenantMemberDto>>> GetAllInternalUsers();
        Task<Result<List<TenantMemberDto>>> GetAllAdminUsersOfTenant();
        Task<Result<List<TenantMemberDto>>> GetAllCustomerUsersOfTenant();
        Task<Result<List<TenantMemberDto>>> GetAllManagerUsersOfTenant();
        Task<Result<List<TenantMemberDto>>> GetAllSupportUsersOfTenant();
    }
}
