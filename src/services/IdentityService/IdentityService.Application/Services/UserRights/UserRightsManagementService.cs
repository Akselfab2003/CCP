using CCP.Shared.AuthContext;
using CCP.Shared.ResultAbstraction;
using IdentityService.Application.Services.Group;
using IdentityService.Application.Services.User;
using Keycloak.Sdk.Models;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Services.UserRights
{
    public class UserRightsManagementService : IUserRightsManagementService
    {
        private readonly ILogger<UserRightsManagementService> _logger;
        private readonly IGroupService _groupService;
        private readonly ICurrentUser _currentUser;
        private readonly IUserService _userService;

        public UserRightsManagementService(ILogger<UserRightsManagementService> logger, IGroupService groupService, IUserService userService, ICurrentUser currentUser)
        {
            _logger = logger;
            _groupService = groupService;
            _userService = userService;
            _currentUser = currentUser;
        }

        public async Task<Result> AssignUserRights(Guid userId,
                                                   string GroupName,
                                                   CancellationToken ct = default)
        {
            try
            {
                var userDetailsResult = await _userService.GetUserDetails(userId, ct);
                if (userDetailsResult.IsFailure)
                {
                    _logger.LogWarning("Failed to retrieve user details for user {UserId}: {ErrorDescription}", userId, userDetailsResult.Error.Description);
                    return Result.Failure(Error.Failure("GetUserDetailsFailed", $"Failed to retrieve user details for user {userId}: {userDetailsResult.Error.Description}"));
                }

                UserKeycloakAccount userDetails = userDetailsResult.Value;

                if (userDetails.Groups != null && userDetails.Groups.Contains(GroupName))
                {
                    _logger.LogInformation("User {UserId} is already a member of group {GroupName}", userId, GroupName);
                    return Result.Failure(Error.Failure("UserAlreadyInGroup", $"User {userId} is already a member of group {GroupName}"));
                }

                var AddUserToGroupResult = await _groupService.AddUserToGroup(GroupName, _currentUser.OrganizationId, userId, ct);
                if (AddUserToGroupResult.IsFailure)
                {
                    _logger.LogWarning("Failed to add user {UserId} to group {GroupName}: {ErrorDescription}", userId, GroupName, AddUserToGroupResult.Error.Description);
                    return Result.Failure(Error.Failure("AddUserToGroupFailed", $"Failed to add user {userId} to group {GroupName}: {AddUserToGroupResult.Error.Description}"));
                }
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while assigning user rights for user {UserId} to group {GroupName}", userId, GroupName);
                return Result.Failure(Error.Failure("AssignUserRightsFailed", $"An error occurred while assigning user rights for user {userId} to group {GroupName}: {ex.Message}"));
            }
        }
    }
}
