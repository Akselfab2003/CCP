using CCP.Shared.ResultAbstraction;
using IdentityService.Sdk.Models;

namespace IdentityService.Sdk.Services.Customer
{
    public interface ICustomerService
    {
        Task<Result<List<TenantMember>>> GetAllCustomers(CancellationToken ct = default);
        Task<Result> InviteCustomer(string Email, CancellationToken ct = default);
    }
}
