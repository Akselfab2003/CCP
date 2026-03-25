using CCP.Sdk.utils.Abstractions;
using CCP.Shared.ResultAbstraction;
using Keycloak.Sdk.ServiceDefaults;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;

namespace Keycloak.Sdk.services.management
{
    internal class ManagementService : IManagementKeycloakService
    {
        private readonly ILogger<ManagementService> _logger;
        private readonly IKiotaApiClient<KeycloakClient> _client;
        private KeycloakClient Client => _client.Client;
        private const int DefaultLifespanSeconds = 5 * 60; // Default lifespan of the email action link (5 minutes)

        public ManagementService(ILogger<ManagementService> logger, IKiotaApiClient<KeycloakClient> client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task<Result> ExecuteEmailRequiredActions(string email, string userId, List<string> actions, string redirectUrl = "https://localhost:7033", int lifespan = DefaultLifespanSeconds, CancellationToken ct = default)
        {
            try
            {
                await Client.Admin.Realms[Constants.REALM].Users[userId.ToString()].ExecuteActionsEmail.PutAsync(actions, req =>
                {
                    req.QueryParameters.ClientId = Constants.DEFAULT_CLIENT_ID;
                    req.QueryParameters.RedirectUri = redirectUrl;
                    req.QueryParameters.Lifespan = lifespan;
                }, cancellationToken: ct);

                return Result.Success();
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "API error executing email required actions for user {UserId} with email {Email}. Status code: {StatusCode}", userId, email, ex.ResponseStatusCode);
                return ex.ResponseStatusCode switch
                {
                    404 => Result.Failure(Error.NotFound("UserNotFound", $"User with ID {userId} and email {email} not found")),
                    400 => Result.Failure(Error.Validation("InvalidRequest", $"Invalid request to execute email required actions for user {userId} with email {email}")),
                    _ => Result.Failure(Error.Failure("EmailRequiredActionsFailed", $"Failed to execute email required actions for user {userId} with email {email}. Status code: {ex.ResponseStatusCode}"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing email required actions for user {UserId} with email {Email}", userId, email);
                return Result.Failure(Error.Failure("EmailRequiredActionsFailed", $"Failed to execute email required actions for user {userId} with email {email}"));
            }
        }

    }
}
