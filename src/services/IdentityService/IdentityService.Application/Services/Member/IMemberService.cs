using CCP.Shared.ResultAbstraction;
using IdentityService.Application.Models;

namespace IdentityService.Application.Services.Member
{
    public interface IMemberService
    {
        Task<Result<List<TenantMemberDto>>> GetAllTenantMembers();
    }
}
