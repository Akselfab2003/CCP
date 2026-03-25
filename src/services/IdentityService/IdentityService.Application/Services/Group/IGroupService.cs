using CCP.Shared.ResultAbstraction;

namespace IdentityService.Application.Services.Group
{
    public interface IGroupService
    {
        Task<Result> AddUserToGroup(string groupName, Guid OrgId, Guid userID, CancellationToken ct);
        Task<Result> CreateDefaultGroupsForOrganization(Guid orgID, CancellationToken ct);
        Task<Result> RemoveUserFromGroup(string groupName, Guid OrgId, Guid userID, CancellationToken ct);
    }
}
