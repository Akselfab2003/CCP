using CCP.Sdk.utils.Abstractions;
using CCP.Shared.ResultAbstraction;
using CCP.Shared.ValueObjects;
using IdentityService.Sdk.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;

namespace IdentityService.Sdk.Services.UserRights
{
    internal class UserRightsManagementClient : IUserRightsManagementService
    {
        private readonly ILogger<UserRightsManagementClient> _logger;
        private readonly IKiotaApiClient<IdentityServiceClient> _apiClient;
        private IdentityServiceClient Client => _apiClient.Client;

        public UserRightsManagementClient(ILogger<UserRightsManagementClient> logger, IKiotaApiClient<IdentityServiceClient> apiClient)
        {
            _logger = logger;
            _apiClient = apiClient;
        }


        public async Task<Result> AssignRightsToUser(Guid UserId, UserRole role, CancellationToken ct = default)
        {
            try
            {
                string groupName = role.ToGroupName();

                var requestBody = new AssignUserRightsRequest
                {
                    UserId = UserId,
                    Role = role.ToString()
                };

                await Client.Userrights.Assign.PostAsync(requestBody,
                                                         cancellationToken: ct);
                return Result.Success();
            }
            catch (ApiException apiEx)
            {
                return apiEx.ResponseStatusCode switch
                {
                    400 => Result.Failure(Error.Validation(code: "InvalidRequest", description: "The request was invalid.")),
                    404 => Result.Failure(Error.NotFound(code: "UserNotFound", description: "The specified user was not found.")),
                    409 => Result.Failure(Error.Conflict(code: "Conflict", description: "There was a conflict with the current state of the resource.")),
                    _ => Result.Failure(Error.Failure(code: "ApiError", description: $"An API error occurred with status code {apiEx.ResponseStatusCode}."))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while assigning rights to user.");
                return Result.Failure(Error.Failure(code: "AssignRightsToUserFailed", description: "An error occurred while assigning rights to user."));
            }

        }
    }
}
