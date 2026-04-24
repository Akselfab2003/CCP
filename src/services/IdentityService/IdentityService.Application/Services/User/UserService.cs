using CCP.Shared.ResultAbstraction;
using Duende.IdentityModel.Client;
using IdentityService.Application.Models;
using Keycloak.Sdk.Models;
using Keycloak.Sdk.services.users;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Services.User
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IUserKeycloakService _userKeycloakService;
        public UserService(ILogger<UserService> logger, IUserKeycloakService userKeycloakService)
        {
            _logger = logger;
            _userKeycloakService = userKeycloakService;
        }

        public async Task<Result<string>> Authenticate(AuthenticatingRequest authenticatingRequest, CancellationToken ct = default)
        {
            try
            {
                var Client = new HttpClient();

                var response = await Client.RequestPasswordTokenAsync(new PasswordTokenRequest()
                {
                    Address = "http://localhost:8080/realms/CCP/protocol/openid-connect/token",
                    ClientId = "CCP",
                    Scope = "openid profile",
                    UserName = authenticatingRequest.UserName,
                    Password = authenticatingRequest.Password
                });

                if (response == null)
                {
                    return Result.Failure<string>(Error.Failure("AuthenticationFailed", $"Authentication failed for user '{authenticatingRequest.UserName}': Response Null"));
                }

                if (response.IsError)
                {
                    _logger.LogError("Authentication failed for user '{Username}': {ErrorDescription}", authenticatingRequest.UserName, response.ErrorDescription);
                    return Result.Failure<string>(Error.Failure("AuthenticationFailed", $"Authentication failed for user '{authenticatingRequest.UserName}': {response.ErrorDescription}"));
                }

                if (string.IsNullOrEmpty(response.AccessToken))
                {
                    _logger.LogError("Authentication succeeded but access token is null or empty for user '{Username}'", authenticatingRequest.UserName);
                    return Result.Failure<string>(Error.Failure("AuthenticationFailed", $"Authentication succeeded but access token is unavailable for user '{authenticatingRequest.UserName}'"));
                }

                return Result.Success(response.AccessToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during authentication for user '{Username}'", authenticatingRequest.UserName);
                return Result.Failure<string>(Error.Failure("AuthenticationFailed", $"An error occurred during authentication for user '{authenticatingRequest.UserName}'"));
            }
        }

        public async Task<Result<Guid>> CreateUser(string email, string firstName, string lastName, string password, CancellationToken ct = default)
        {
            try
            {
                return await _userKeycloakService.CreateUser(email, firstName, lastName, password, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a user with email {Email}", email);
                return Result.Failure<Guid>(Error.Failure("FailedToCreateUser", $"An error occurred while creating a user with email {email}"));
            }
        }

        public async Task<Result<Guid>> CreateCustomer(string email, CancellationToken ct = default)
        {
            try
            {
                return await _userKeycloakService.CreateCustomer(email, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a customer with email {Email}", email);
                return Result.Failure<Guid>(Error.Failure("FailedToCreateCustomer", $"An error occurred while creating a customer with email {email}"));
            }
        }

        public async Task<Result<Guid>> CreateSupporter(string email, CancellationToken ct = default)
        {
            try
            {
                return await _userKeycloakService.CreateSupporter(email, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a supporter with email {Email}", email);
                return Result.Failure<Guid>(Error.Failure("FailedToCreateSupporter", $"An error occurred while creating a supporter with email {email}"));
            }
        }



        public async Task<Result<UserKeycloakAccount>> GetUserDetails(Guid UserID, CancellationToken ct)
        {
            try
            {
                return await _userKeycloakService.GetUserDetailsByID(UserID, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting user details for user with ID {UserID}", UserID);
                return Result.Failure<UserKeycloakAccount>(Error.Failure("FailedToGetUserDetails", $"An error occurred while getting user details for user with ID {UserID}"));
            }
        }

        public async Task<Result<List<UserKeycloakAccount>>> SearchUsers(string searchTerm, CancellationToken ct)
        {
            try
            {
                return await _userKeycloakService.SearchUsers(searchTerm, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while searching for users with searchTerm '{searchTerm}'", searchTerm);
                return Result.Failure<List<UserKeycloakAccount>>(Error.Failure("FailedToSearchUsers", $"An error occurred while searching for users with searchTerm '{searchTerm}'"));
            }
        }
    }
}
