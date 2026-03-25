using CCP.Shared.ResultAbstraction;
using IdentityService.Sdk.Models;

namespace IdentityService.Sdk.Services.User
{
    public interface IUserService
    {
        Task<Result<UserAccount>> GetUserDetailsAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<Result<List<UserAccount>>> SearchUsers(string SearchTerm, CancellationToken ct = default);
    }
}
