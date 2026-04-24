using CCP.Shared.ResultAbstraction;

namespace IdentityService.Application.Services.UserRights
{
    public interface IUserRightsManagementService
    {
        Task<Result> AssignUserRights(Guid userId, string GroupName, CancellationToken ct = default);
    }
}