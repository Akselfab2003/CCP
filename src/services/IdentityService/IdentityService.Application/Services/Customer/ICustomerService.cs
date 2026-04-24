using CCP.Shared.ResultAbstraction;
using IdentityService.Application.Models;

namespace IdentityService.Application.Services.Customer
{
    public interface ICustomerService
    {
        Task<Result<List<TenantMemberDto>>> GetAllTenantCustomerUsers(CancellationToken ct = default);
        Task<Result<Guid>> InviteCustomer(string Email, CancellationToken ct = default);
    }
}
