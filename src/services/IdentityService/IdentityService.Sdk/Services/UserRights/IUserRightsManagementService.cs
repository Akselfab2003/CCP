using CCP.Shared.ResultAbstraction;
using CCP.Shared.ValueObjects;

namespace IdentityService.Sdk.Services.UserRights
{
    public interface IUserRightsManagementService
    {
        Task<Result> AssignRightsToUser(Guid UserId, UserRole role, CancellationToken ct = default);
    }
}
