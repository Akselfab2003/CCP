using CCP.Shared.ResultAbstraction;

namespace Keycloak.Sdk.services.groups
{
    public interface IGroupKeycloakService
    {
        Task<Result> AddUserToGroup(Guid TenantID, string UserID, string GroupName);
        Task<Result<Guid>> CreateGroupAsync(Guid? ParentGroupID, string groupName, List<string>? Roles = null, CancellationToken ct = default);
        Task<Result> RemoveUserFromGroup(Guid TenantID, string UserID, string GroupName);
    }
}
