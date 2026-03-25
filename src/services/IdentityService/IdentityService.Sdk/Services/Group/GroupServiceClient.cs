using CCP.Sdk.utils.Abstractions;
using Microsoft.Extensions.Logging;

namespace IdentityService.Sdk.Services.Group
{
    internal class GroupServiceClient : IGroupService
    {
        private readonly ILogger<GroupServiceClient> _logger;
        private readonly IKiotaApiClient<IdentityServiceClient> _apiClient;

        private IdentityServiceClient Client => _apiClient.Client;

        public GroupServiceClient(IKiotaApiClient<IdentityServiceClient> apiClient, ILogger<GroupServiceClient> logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        //public async Task<Result> CreateGroupAsync(string groupName, CancellationToken cancellationToken = default)
        //{
        //    try
        //    {
        //        await Client.Group.Create.PostAsync(requestConfiguration =>
        //        {
        //            requestConfiguration.QueryParameters.GroupName = groupName;
        //        });

        //        return Result.Success();
        //    }
        //    catch (ApiException ex)
        //    {
        //        return ex.ResponseStatusCode switch
        //        {
        //            400 => Result.Failure(Error.Validation("InvalidGroupName", $"The group name '{groupName}' is invalid.")),
        //            409 => Result.Failure(Error.Conflict("GroupAlreadyExists", $"A group with the name '{groupName}' already exists.")),
        //            _ => Result.Failure(Error.Failure("CreateGroupFailed", $"An error occurred while creating the group: {ex.Message}"))
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error creating group {GroupName}", groupName);
        //        return Result.Failure(Error.Failure("CreateGroupFailed", $"An error occurred while creating the group: {ex.Message}"));
        //    }
        //}

        //public async Task<Result> AddUserToGroup(string groupName, Guid UserID, CancellationToken cancellationToken = default)
        //{
        //    try
        //    {
        //        await Client.Group[groupName].AddUser[UserID].PostAsync(cancellationToken: cancellationToken);
        //        return Result.Success();
        //    }
        //    catch (ApiException ex)
        //    {
        //        return ex.ResponseStatusCode switch
        //        {
        //            400 => Result.Failure(Error.Validation("InvalidUserID", $"The user ID '{UserID}' is invalid.")),
        //            404 => Result.Failure(Error.NotFound("GroupNotFound", $"The group '{groupName}' was not found.")),
        //            409 => Result.Failure(Error.Conflict("UserAlreadyInGroup", $"The user with ID '{UserID}' is already a member of the group '{groupName}'.")),
        //            _ => Result.Failure(Error.Failure("AddUserToGroupFailed", $"An error occurred while adding the user to the group: {ex.Message}"))
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error adding user {UserID} to group {GroupName}", UserID, groupName);
        //        return Result.Failure(Error.Failure("AddUserToGroupFailed", $"An error occurred while adding the user to the group: {ex.Message}"));
        //    }
        //}

        //public async Task<Result> RemoveUserFromGroup(string groupName, Guid UserID, CancellationToken cancellationToken = default)
        //{
        //    try
        //    {
        //        await Client.Group[groupName].RemoveUser[UserID].PostAsync(cancellationToken: cancellationToken);
        //        return Result.Success();
        //    }
        //    catch (ApiException ex)
        //    {
        //        return ex.ResponseStatusCode switch
        //        {
        //            400 => Result.Failure(Error.Validation("InvalidUserID", $"The user ID '{UserID}' is invalid.")),
        //            404 => Result.Failure(Error.NotFound("GroupNotFound", $"The group '{groupName}' was not found.")),
        //            409 => Result.Failure(Error.Conflict("UserNotInGroup", $"The user with ID '{UserID}' is not a member of the group '{groupName}'.")),
        //            _ => Result.Failure(Error.Failure("RemoveUserFromGroupFailed", $"An error occurred while removing the user from the group: {ex.Message}"))
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error removing user {UserID} from group {GroupName}", UserID, groupName);
        //        return Result.Failure(Error.Failure("RemoveUserFromGroupFailed", $"An error occurred while removing the user from the group: {ex.Message}"));
        //    }
        //}
    }
}

