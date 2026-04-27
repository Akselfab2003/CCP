using CCP.Sdk.utils.Abstractions;
using CCP.Shared.ResultAbstraction;
using Keycloak.Sdk.Models;
using Keycloak.Sdk.ServiceDefaults;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;

namespace Keycloak.Sdk.services.users
{
    internal class UserServices : IUserKeycloakService
    {
        private readonly ILogger<UserServices> _logger;
        private readonly IKiotaApiClient<KeycloakClient> _apiClient;
        private KeycloakClient Client => _apiClient.Client;
        public UserServices(ILogger<UserServices> logger, IKiotaApiClient<KeycloakClient> apiClient)
        {
            _logger = logger;
            _apiClient = apiClient;
        }

        public async Task<Result<Guid>> CreateUser(string email, string firstName, string lastName, string? password = null, CancellationToken ct = default)
        {
            try
            {
                var CredentialList = new List<CredentialRepresentation>();
                if (!string.IsNullOrEmpty(password))
                {
                    CredentialList.Add(new CredentialRepresentation
                    {
                        Type = "password",
                        Value = password,
                        Temporary = false
                    });
                }

                var newUser = new UserRepresentation
                {
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    Username = email,
                    Credentials = CredentialList,
                    Enabled = true
                };

                await Client.Admin.Realms[Constants.REALM].Users.PostAsync(newUser, cancellationToken: ct);

                var createdUser = await Client.Admin.Realms[Constants.REALM].Users.GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Email = email;
                    requestConfiguration.QueryParameters.Max = 1;
                }, cancellationToken: ct);

                if (createdUser == null || createdUser.Count == 0)
                {
                    _logger.LogWarning("User created but not found with email: {Email}", email);
                    return Result.Failure<Guid>(Error.Failure("CreateUser.failed", "User was created but could not be retrieved"));
                }

                if (Guid.TryParse(createdUser[0].Id, out var userId))
                {
                    return Result.Success(userId);
                }
                else
                {
                    _logger.LogWarning("User created but has invalid ID format: {UserId}", createdUser[0].Id);
                    return Result.Failure<Guid>(Error.Failure("CreateUser.failed", "User was created but has an invalid ID format"));
                }
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error creating user with email: {Email}", email);
                return Result.Failure<Guid>(Error.Failure("CreateUser.failed", $"An error occurred while creating the user: {ex.Message}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating user with email: {Email}", email);
                return Result.Failure<Guid>(Error.Failure("CreateUser.failed", $"An unexpected error occurred: {ex.Message}"));
            }
        }


        public async Task<Result<Guid>> CreateCustomer(string email, CancellationToken ct = default)
        {
            try
            {
                var newUser = new UserRepresentation
                {
                    Email = email,
                    FirstName = "",
                    LastName = "",
                    Username = email,
                    Enabled = true,
                };

                await Client.Admin.Realms[Constants.REALM].Users.PostAsync(newUser, cancellationToken: ct);

                var createdUser = await Client.Admin.Realms[Constants.REALM].Users.GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Email = email;
                    requestConfiguration.QueryParameters.Max = 1;
                }, cancellationToken: ct);

                if (createdUser == null || createdUser.Count == 0)
                {
                    _logger.LogWarning("User created but not found with email: {Email}", email);
                    return Result.Failure<Guid>(Error.Failure("CreateUser.failed", "User was created but could not be retrieved"));
                }

                if (Guid.TryParse(createdUser[0].Id, out var userId))
                {
                    return Result.Success(userId);
                }
                else
                {
                    _logger.LogWarning("User created but has invalid ID format: {UserId}", createdUser[0].Id);
                    return Result.Failure<Guid>(Error.Failure("CreateUser.failed", "User was created but has an invalid ID format"));
                }
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error creating user with email: {Email}", email);
                return Result.Failure<Guid>(Error.Failure("CreateUser.failed", $"An error occurred while creating the user: {ex.Message}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating user with email: {Email}", email);
                return Result.Failure<Guid>(Error.Failure("CreateUser.failed", $"An unexpected error occurred: {ex.Message}"));
            }
        }


        public async Task<Result<Guid>> CreateSupporter(string email, CancellationToken ct = default)
        {
            try
            {
                var newUser = new UserRepresentation
                {
                    Email = email,
                    FirstName = "",
                    LastName = "",
                    Username = email,
                    Enabled = true,
                };

                await Client.Admin.Realms[Constants.REALM].Users.PostAsync(newUser, cancellationToken: ct);

                var createdUser = await Client.Admin.Realms[Constants.REALM].Users.GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Email = email;
                    requestConfiguration.QueryParameters.Max = 1;
                }, cancellationToken: ct);

                if (createdUser == null || createdUser.Count == 0)
                {
                    _logger.LogWarning("User created but not found with email: {Email}", email);
                    return Result.Failure<Guid>(Error.Failure("CreateSupporter.failed", "User was created but could not be retrieved"));
                }

                if (Guid.TryParse(createdUser[0].Id, out var userId))
                {
                    return Result.Success(userId);
                }
                else
                {
                    _logger.LogWarning("User created but has invalid ID format: {UserId}", createdUser[0].Id);
                    return Result.Failure<Guid>(Error.Failure("CreateSupporter.failed", "User was created but has an invalid ID format"));
                }
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error creating user with email: {Email}", email);
                return Result.Failure<Guid>(Error.Failure("CreateSupporter.failed", $"An error occurred while creating the user: {ex.Message}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating user with email: {Email}", email);
                return Result.Failure<Guid>(Error.Failure("CreateSupporter.failed", $"An unexpected error occurred: {ex.Message}"));
            }
        }



        public async Task<Result<UserKeycloakAccount>> GetUserDetailsByID(Guid UserID, CancellationToken ct = default)
        {
            try
            {
                var user = await Client.Admin.Realms[Constants.REALM]
                                             .Users[UserID.ToString()]
                                             .GetAsync(cancellationToken: ct);

                var userGroups = await Client.Admin.Realms[Constants.REALM]
                                                  .Users[UserID.ToString()]
                                                  .Groups
                                                  .GetAsync(cancellationToken: ct);

                if (user is null)
                    return Result.Failure<UserKeycloakAccount>(Error.Failure("GetUserDetailsByID.failed", "User not found"));

                if (string.IsNullOrEmpty(user.Id))
                    return Result.Failure<UserKeycloakAccount>(Error.Failure("GetUserDetailsByID.failed", "User ID is missing"));

                if (string.IsNullOrEmpty(user.Email))
                    return Result.Failure<UserKeycloakAccount>(Error.Failure("GetUserDetailsByID.failed", "User email is missing"));


                if (user.RealmRoles == null)
                    user.RealmRoles = [];

                var groups = new List<string>();

                if (userGroups != null)
                {
                    foreach (var group in userGroups)
                    {
                        if (!string.IsNullOrEmpty(group.Name))
                            groups.Add(group.Name);
                    }
                }

                return Result.Success(new UserKeycloakAccount()
                {
                    Id = Guid.Parse(user.Id),
                    Email = user.Email,
                    Name = $"{user.FirstName ?? ""} {user.LastName ?? ""}".Trim(),
                    Enabled = user.Enabled,
                    CreatedTimestamp = user.CreatedTimestamp,
                    RealmRoles = user.RealmRoles,
                    Groups = groups
                });
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error fetching user details for UserID: {UserID}", UserID);
                return ex.ResponseStatusCode switch
                {
                    (int)System.Net.HttpStatusCode.NotFound => Result.Failure<UserKeycloakAccount>(Error.Failure("GetUserDetailsByID.failed", "User not found")),
                    _ => Result.Failure<UserKeycloakAccount>(Error.Failure("GetUserDetailsByID.failed", $"An error occurred while fetching user details: {ex.Message}"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching user details for UserID: {UserID}", UserID);
                return Result.Failure<UserKeycloakAccount>(Error.Failure("GetUserDetailsByID.failed", $"An unexpected error occurred: {ex.Message}"));
            }
        }

        public async Task<Result<List<UserKeycloakAccount>>> SearchUsers(string searchTerm, CancellationToken ct)
        {
            try
            {
                // Hvis searchTerm er tom eller whitespace, brug "*" eller null for at hente alle brugere
                var search = string.IsNullOrWhiteSpace(searchTerm) ? "*" : searchTerm;

                var users = await Client.Admin.Realms[Constants.REALM].Users.GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Search = search;
                    requestConfiguration.QueryParameters.Max = 100; // Øg max til 100 for at få flere brugere
                }, cancellationToken: ct);

                if (users == null || users.Count == 0)
                    return Result.Success(new List<UserKeycloakAccount>());

                var userAccounts = users
                    .Where(user => !string.IsNullOrEmpty(user.Id)) // Filtrer brugere uden ID
                    .Select(user => new UserKeycloakAccount
                    {
                        Id = Guid.TryParse(user.Id, out var id) ? id : Guid.Empty,
                        Email = user.Email ?? string.Empty,
                        Name = $"{user.FirstName ?? ""} {user.LastName ?? ""}".Trim(),
                        Enabled = user.Enabled,
                        CreatedTimestamp = user.CreatedTimestamp,
                        RealmRoles = user.RealmRoles,
                        Groups = user.Groups
                    })
                    .Where(u => u.Id != Guid.Empty) // Filtrer brugere med ugyldige IDs
                    .ToList();

                return Result.Success(userAccounts);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "Error searching for users with searchTerm: {SearchTerm}", searchTerm);
                return Result.Failure<List<UserKeycloakAccount>>(Error.Failure("SearchUsers.failed", $"An error occurred while searching for users: {ex.Message}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error searching for users with searchTerm: {SearchTerm}", searchTerm);
                return Result.Failure<List<UserKeycloakAccount>>(Error.Failure("SearchUsers.failed", $"An unexpected error occurred: {ex.Message}"));
            }
        }
    }
}
