using CCP.Shared.ResultAbstraction;
using Keycloak.Sdk.services.groups;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Services.Group
{
    public class GroupService : IGroupService
    {
        private readonly ILogger<GroupService> _logger;
        private readonly IGroupKeycloakService _groupKeycloakService;

        public GroupService(ILogger<GroupService> logger, IGroupKeycloakService groupKeycloakService)
        {
            _logger = logger;
            _groupKeycloakService = groupKeycloakService;
        }

        public async Task<Result> AddUserToGroup(string groupName, Guid OrgId, Guid userID, CancellationToken ct)
        {
            try
            {
                return await _groupKeycloakService.AddUserToGroup(OrgId, userID.ToString(), groupName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding user with ID {UserID} to group '{GroupName}'", userID, groupName);
                return Result.Failure(Error.Failure("AddUserToGroup.failed", $"An error occurred while adding the user to the group: {ex.Message}"));
            }
        }

        public async Task<Result> RemoveUserFromGroup(string groupName, Guid OrgId, Guid userID, CancellationToken ct)
        {
            try
            {
                return await _groupKeycloakService.RemoveUserFromGroup(OrgId, userID.ToString(), groupName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while removing user with ID {UserID} from group '{GroupName}'", userID, groupName);
                return Result.Failure(Error.Failure("RemoveUserFromGroup.failed", $"An error occurred while removing the user from the group: {ex.Message}"));
            }
        }

        public async Task<Result> CreateDefaultGroupsForOrganization(Guid orgID, CancellationToken ct)
        {
            try
            {
                var GroupsAndRole = new List<(string GroupName, string RoleName)>
                {
                    ("Admins", "org.Admin"),
                    ("Managers", "org.Manager"),
                    ("Supporters", "org.Supporter"),
                    ("Customers","org.Customer"),
                };

                var ParentGroupResult = await _groupKeycloakService.CreateGroupAsync(ParentGroupID: null,
                                                                                     groupName: $"org-{orgID.ToString()}",
                                                                                     ct: ct);

                if (ParentGroupResult.IsFailure)
                    return Result.Failure(Error.Failure("CreateParentGroup.failed", $"Failed to create parent group for organization: {ParentGroupResult.Error.Description}"));

                foreach (var (GroupName, RoleName) in GroupsAndRole)
                {
                    var GroupResult = await _groupKeycloakService.CreateGroupAsync(ParentGroupID: ParentGroupResult.Value,
                                                                                   groupName: GroupName,
                                                                                   Roles: [RoleName],
                                                                                   ct: ct);
                    if (GroupResult.IsFailure)
                        return Result.Failure(Error.Failure("CreateChildGroup.failed", $"Failed to create group '{GroupName}' for organization: {GroupResult.Error.Description}"));
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating default groups for organization with ID {OrgID}", orgID);
                return Result.Failure(Error.Failure("CreateDefaultGroupsForOrganization.failed", $"An error occurred while creating default groups for organization: {ex.Message}"));
            }
        }
    }
}
