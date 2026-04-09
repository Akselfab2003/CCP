using CCP.Sdk.utils.Abstractions;
using CCP.Shared.ResultAbstraction;
using IdentityService.Sdk.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;

namespace IdentityService.Sdk.Services.User
{
    internal class UserServiceClient : IUserService
    {
        private readonly ILogger<UserServiceClient> _logger;
        private readonly IKiotaApiClient<IdentityServiceClient> _apiClient;

        private IdentityServiceClient Client => _apiClient.Client;
        public UserServiceClient(ILogger<UserServiceClient> logger, IKiotaApiClient<IdentityServiceClient> apiClient)
        {
            _logger = logger;
            _apiClient = apiClient;
        }


        public async Task<Result<UserAccount>> GetUserDetailsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var userDetails = await Client.User[userId].GetAsync(cancellationToken: cancellationToken);
                if (userDetails == null)
                {
                    return Result.Failure<UserAccount>(Error.NotFound("UserNotFound", $"User with ID {userId} was not found."));
                }

                if (!userDetails.Id.HasValue)
                {
                    return Result.Failure<UserAccount>(Error.Validation("InvalidUserData", $"User data for ID {userId} is missing a valid ID."));
                }

                if (string.IsNullOrEmpty(userDetails.Name) || string.IsNullOrEmpty(userDetails.Email))
                {
                    return Result.Failure<UserAccount>(Error.Validation("InvalidUserData", $"User data for ID {userId} is incomplete."));
                }

                // Extract createdTimestamp from UntypedNode
                // Extract createdTimestamp from UntypedNode
                long? timestamp = null;
                if (userDetails.CreatedTimestamp != null)
                {
                    try
                    {
                        var timestampValue = userDetails.CreatedTimestamp.GetValue();
                        if (timestampValue is long longValue)
                            timestamp = longValue;
                        else if (timestampValue is int intValue)
                            timestamp = intValue;
                    }
                    catch (NotImplementedException)
                    {
                        // Kiota UntypedNode.GetValue() is not implemented for createdTimestamp
                        // Safe to ignore — timestamp is optional and only used for display
                    }
                }

                Models.UserAccount userAccount = new(
                    userDetails.Id.Value,
                    userDetails.Name,
                    userDetails.Email,
                    userDetails.Enabled,
                    timestamp,
                    userDetails.RealmRoles,
                    userDetails.Groups
                );

                return Result.Success(userAccount);
            }
            catch (ApiException ex)
            {
                return ex.ResponseStatusCode switch
                {
                    404 => Result.Failure<UserAccount>(Error.NotFound("UserNotFound", $"User with ID {userId} was not found.")),
                    400 => Result.Failure<UserAccount>(Error.Validation("InvalidUserId", $"The provided user ID {userId} is invalid.")),
                    _ => Result.Failure<UserAccount>(Error.Failure("GetUserDetailsFailed", $"An error occurred while fetching user details: {ex.Message}"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching details for user {UserId}", userId);
                return Result.Failure<UserAccount>(Error.Failure("GetUserDetailsFailed", $"An error occurred while fetching user details: {ex.Message}"));
            }
        }

        public async Task<Result<List<UserAccount>>> SearchUsers(string SearchTerm, CancellationToken ct = default)
        {
            try
            {
                var userSearchResults = await Client.User.Search.GetAsync(req =>
                {
                    req.QueryParameters.SearchTerm = SearchTerm;
                }, cancellationToken: ct);

                if (userSearchResults == null || userSearchResults.Count == 0)
                {
                    return Result.Failure<List<UserAccount>>(Error.NotFound("NoUsersFound", $"No users found matching search term '{SearchTerm}'."));
                }

                var userAccounts = userSearchResults
                    .Where(u => u.Id.HasValue && !string.IsNullOrEmpty(u.Name) && !string.IsNullOrEmpty(u.Email))
                    .Select(u => new UserAccount(
                        u.Id!.Value,
                        u.Name!,
                        u.Email!,
                        u.Enabled,
                        null,  // CreatedTimestamp - UntypedNode.GetValue() er ikke implementeret endnu
                        u.RealmRoles,
                        u.Groups
                    ))
                    .ToList();
                return Result.Success(userAccounts);
            }
            catch (ApiException ex)
            {
                return ex.ResponseStatusCode switch
                {
                    400 => Result.Failure<List<UserAccount>>(Error.Validation("InvalidSearchTerm", $"The search term '{SearchTerm}' is invalid.")),
                    _ => Result.Failure<List<UserAccount>>(Error.Failure("SearchUsersFailed", $"An error occurred while searching for users: {ex.Message}"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for users with term {SearchTerm}", SearchTerm);
                return Result.Failure<List<UserAccount>>(Error.Failure("SearchUsersFailed", $"An error occurred while searching for users: {ex.Message}"));
            }
        }
    }
}
