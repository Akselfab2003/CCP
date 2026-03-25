using CCP.Sdk.utils.Abstractions;
using CCP.Shared.ResultAbstraction;
using Keycloak.Sdk.Models;
using Keycloak.Sdk.ServiceDefaults;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;

namespace Keycloak.Sdk.services.groups
{
    internal class GroupServices : IGroupKeycloakService
    {
        private readonly ILogger<GroupServices> _logger;
        private readonly IKiotaApiClient<KeycloakClient> _apiClient;
        private KeycloakClient Client => _apiClient.Client;
        public GroupServices(ILogger<GroupServices> logger, IKiotaApiClient<KeycloakClient> apiClient)
        {
            _logger = logger;
            _apiClient = apiClient;
        }

        private async Task<Result<List<GroupRepresentation>>> QueryGroups(string GroupName, CancellationToken ct = default)
        {
            try
            {
                var groups = await Client.Admin.Realms[Constants.REALM].Groups.GetAsync(g =>
                {
                    g.QueryParameters = new()
                    {
                        Q = GroupName,
                        Exact = true,
                        BriefRepresentation = false,
                    };
                }, ct);

                if (groups == null || groups.Count == 0)
                {
                    return Result.Failure<List<GroupRepresentation>>(Error.Failure("GroupNotFound", $"No groups found in Keycloak with name {GroupName}"));
                }
                return Result.Success(groups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to query groups with name {GroupName}. Error: {ErrorMessage}", GroupName, ex.Message);
                return Result.Failure<List<GroupRepresentation>>(Error.Failure("QueryGroupsFailed", $"Failed to query groups from Keycloak with name {GroupName}"));
            }
        }

        public async Task<Result<Guid>> CreateGroupAsync(Guid? ParentGroupID,
                                                   string groupName,
                                                   List<string>? Roles = null,
                                                   CancellationToken ct = default)
        {
            try
            {
                var groupRepresentation = new GroupRepresentation
                {
                    Name = groupName,
                    ParentId = ParentGroupID.HasValue ? ParentGroupID.Value.ToString() : null,
                };

                if (ParentGroupID.HasValue)
                {
                    await Client.Admin.Realms[Constants.REALM].Groups[ParentGroupID.Value.ToString()].Children.PostAsync(groupRepresentation,
                                                                                                                                cancellationToken: ct);
                }
                else
                {
                    await Client.Admin.Realms[Constants.REALM].Groups.PostAsync(groupRepresentation,
                                                                                        cancellationToken: ct);
                }

                var groupsResult = await QueryGroups(groupName, ct);


                if (groupsResult.IsFailure)
                {
                    return Result.Failure<Guid>(groupsResult.Error);
                }

                Guid groupID = Guid.Empty;

                if (ParentGroupID.HasValue)
                {
                    var ParentGroupResult = groupsResult.Value.FirstOrDefault(g => g.Id == ParentGroupID.Value.ToString());

                    if (ParentGroupResult == null)
                    {
                        return Result.Failure<Guid>(Error.Failure("GroupCreationFailed", $"Parent group with ID {ParentGroupID.Value} was not found in Keycloak after creation of child group {groupName}"));
                    }

                    var childGroup = ParentGroupResult.SubGroups?.FirstOrDefault(g => g.Name == groupName);

                    if (childGroup == null)
                    {
                        return Result.Failure<Guid>(Error.Failure("GroupCreationFailed", $"Group with name {groupName} was not found as a child of parent group with ID {ParentGroupID.Value} in Keycloak after creation"));
                    }

                    groupID = Guid.Parse(childGroup.Id!);
                }
                else
                {
                    var Group = groupsResult.Value.FirstOrDefault(g => g.Name == groupName);

                    if (Group == null)
                    {
                        return Result.Failure<Guid>(Error.Failure("GroupCreationFailed", $"Group with name {groupName} was not found in Keycloak after creation"));
                    }

                    groupID = Guid.Parse(Group.Id!);
                }

                if (Roles != null && Roles.Count > 0)
                {
                    var addRolesResult = await AddRolesToGroup(groupID, Roles, ct);
                    if (addRolesResult.IsFailure)
                    {
                        return Result.Failure<Guid>(addRolesResult.Error);
                    }
                }

                return Result.Success(groupID);
            }
            catch (ApiException apiEx)
            {
                return apiEx.ResponseStatusCode switch
                {
                    404 => Result.Failure<Guid>(Error.NotFound("CreateGroupAsync.notfound", $"The specified realm was not found: {Constants.REALM}")),
                    409 => Result.Failure<Guid>(Error.Conflict("CreateGroupAsync.conflict", $"A group with the name '{groupName}' already exists.")),
                    _ => Result.Failure<Guid>(Error.Failure("CreateGroupAsync.failed", $"An error occurred while creating the group: {apiEx.Message}"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating group with name: {GroupName}", groupName);
                return Result.Failure<Guid>(Error.Failure("CreateGroupAsync.failed", $"An error occurred while creating the group: {ex.Message}"));
            }
        }

        private async Task<Result<Guid>> GetClientId(string ClientName, CancellationToken cancellationToken)
        {
            try
            {
                var clients = await Client.Admin.Realms[Constants.REALM]
                                         .Clients
                                         .GetAsync(req =>
                                         {
                                             req.QueryParameters.Search = true;
                                             req.QueryParameters.Q = ClientName;
                                         }, cancellationToken: cancellationToken);

                if (clients == null || !clients.Any())
                    return Result.Failure<Guid>(Error.NotFound("GetClientId.notfound", $"No clients found in realm: {Constants.REALM}"));

                var client = clients.Where(c => c != null && !string.IsNullOrEmpty(c.Name))
                                    .SingleOrDefault(c => c.Name!.Equals(ClientName, StringComparison.OrdinalIgnoreCase));

                if (client == null)
                    return Result.Failure<Guid>(Error.NotFound("GetClientId.notfound", $"Client with name '{ClientName}' not found in realm: {Constants.REALM}"));


                if (!Guid.TryParse(client.Id, out var clientId))
                    return Result.Failure<Guid>(Error.Failure("GetClientId.failed", $"Client found but has invalid ID format: {client.Id}"));

                return Result.Success(clientId);
            }
            catch (ApiException apiEx)
            {
                return apiEx.ResponseStatusCode switch
                {
                    404 => Result.Failure<Guid>(Error.NotFound("GetClientId.notfound", $"Client with name '{ClientName}' not found in realm: {Constants.REALM}")),
                    _ => Result.Failure<Guid>(Error.Failure("GetClientId.failed", $"An error occurred while retrieving the client ID: {apiEx.Message}"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving client ID for client: {ClientName}", ClientName);
                return Result.Failure<Guid>(Error.Failure("GetClientId.failed", $"An error occurred while retrieving the client ID: {ex.Message}"));
            }
        }

        private async Task<Result<RoleRepresentation>> GetRole(Guid ClientId, string roleName)
        {
            try
            {
                var roles = await Client.Admin.Realms[Constants.REALM]
                                         .Clients[ClientId.ToString()]
                                         .Roles[roleName]
                                         .GetAsync();

                if (roles == null)
                    return Result.Failure<RoleRepresentation>(Error.NotFound("GetRole.notfound", $"Role with name '{roleName}' not found for client with ID: {ClientId}"));

                return Result.Success(roles);
            }
            catch (ApiException apiEx)
            {
                return apiEx.ResponseStatusCode switch
                {
                    404 => Result.Failure<RoleRepresentation>(Error.NotFound("GetRole.notfound", $"Role with name '{roleName}' not found for client with ID: {ClientId}")),
                    _ => Result.Failure<RoleRepresentation>(Error.Failure("GetRole.failed", $"An error occurred while retrieving the role: {apiEx.Message}"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role: {RoleName} for client with ID: {ClientId}", roleName, ClientId);
                return Result.Failure<RoleRepresentation>(Error.Failure("GetRole.failed", $"An error occurred while retrieving the role: {ex.Message}"));
            }

        }

        private async Task<Result> AddRolesToGroup(Guid groupId, List<string> Roles, CancellationToken ct)
        {
            try
            {

                var clientId = await GetClientId(Constants.DEFAULT_CLIENT_GROUP_NAME, ct);
                if (clientId.IsFailure) return Result.Failure(Error.Failure("AddRoleToGroup.failed", $"Failed to retrieve client ID: {clientId.Error.Description}"));

                var roles = new List<RoleRepresentation>();

                foreach (var roleName in Roles)
                {
                    var roleResult = await GetRole(clientId.Value, roleName);
                    if (roleResult.IsFailure) return Result.Failure(Error.Failure("AddRoleToGroup.failed", $"Failed to retrieve role: {roleResult.Error.Description}"));
                    roles.Add(roleResult.Value);
                }

                await Client.Admin.Realms[Constants.REALM]
                                  .Groups[groupId.ToString()]
                                  .RoleMappings
                                  .Clients[clientId.Value.ToString()]
                                  .PostAsync(roles, cancellationToken: ct);

                return Result.Success();
            }
            catch (ApiException apiEx)
            {
                return apiEx.ResponseStatusCode switch
                {
                    404 => Result.Failure(Error.NotFound("AddRoleToGroup.notfound", $"The specified client was not found: ClientName={Constants.DEFAULT_CLIENT_GROUP_NAME}")),
                    409 => Result.Failure(Error.Conflict("AddRoleToGroup.conflict", $"One or more roles already exist in the group.")),
                    403 => Result.Failure(Error.Failure("AddRoleToGroup.forbidden", $"You do not have permission to add roles to this group.")),
                    _ => Result.Failure(Error.Failure("AddRoleToGroup.failed", $"An error occurred while adding the role to the group: {apiEx.Message}"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding roles to group with ID: {GroupId}", groupId);
                return Result.Failure(Error.Failure("AddRoleToGroup.failed", $"An error occurred while adding the role to the group: {ex.Message}"));
            }
        }

        public async Task<Result<KeycloakGroup>> GetAllTenantGroups(Guid TenantID, CancellationToken ct = default)
        {
            try
            {
                var rootGroupName = "org-" + TenantID.ToString();

                var groupsResult = await QueryGroups(rootGroupName, ct);

                if (groupsResult.IsFailure)
                {
                    return Result.Failure<KeycloakGroup>(groupsResult.Error);
                }

                var groupRepresentations = groupsResult.Value;

                if (groupRepresentations.Count == 0)
                    return Result.Failure<KeycloakGroup>(Error.Failure("GroupNotFound", $"No groups found in Keycloak with name {rootGroupName} for tenant with ID {TenantID}"));


                var ParentGroup = groupRepresentations.FirstOrDefault(g => g.Name == rootGroupName);

                if (ParentGroup == null)
                    return Result.Failure<KeycloakGroup>(Error.Failure("GroupNotFound", $"No groups found in Keycloak with name {rootGroupName} for tenant with ID {TenantID}"));

                KeycloakGroup MapGroup(GroupRepresentation group)
                {
                    return new KeycloakGroup
                    {
                        Id = Guid.TryParse(group.Id, out var id) ? id : Guid.Empty,
                        Name = group.Name,
                        Roles = group.RealmRoles ?? [],
                        ParentId = Guid.TryParse(group.ParentId, out Guid parentID) ? parentID : Guid.Empty,
                        SubGroups = group.SubGroups?.Select(MapGroup).ToList() ?? []
                    };
                }

                var resultGroup = MapGroup(ParentGroup);
                return Result.Success(resultGroup);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve group information for tenant with ID {TenantID}. Error: {ErrorMessage}", TenantID, ex.Message);
                return Result.Failure<KeycloakGroup>(Error.Failure("GetGroupFailed", $"Failed to retrieve group information from Keycloak for tenant with ID {TenantID}"));
            }
        }
        public async Task<Result> AddUserToGroup(Guid TenantID, string UserID, string GroupName)
        {
            try
            {

                var tenantGroups = await GetAllTenantGroups(TenantID);
                if (tenantGroups.IsFailure)
                {
                    return Result.Failure(tenantGroups.Error);
                }

                var group = tenantGroups.Value.SubGroups.FirstOrDefault(g => g.Name == GroupName);

                if (group == null)
                {
                    return Result.Failure(Error.Failure("GroupNotFound", $"Group with name {GroupName} was not found for tenant with ID {TenantID} in Keycloak"));
                }

                await Client.Admin.Realms[Constants.REALM]
                                  .Users[UserID]
                                  .Groups[group.Id.ToString()]
                                  .PutAsync();

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add user with ID {UserID} to group with {GroupName}. Error: {ErrorMessage}", UserID, GroupName, ex.Message);
                return Result.Failure(Error.Failure("AddUserToGroupFailed", $"Failed to add user with ID {UserID} to group with Name {GroupName} in Keycloak"));
            }
        }

        public async Task<Result> RemoveUserFromGroup(Guid TenantID, string UserID, string GroupName)
        {
            try
            {
                var tenantGroups = await GetAllTenantGroups(TenantID);
                if (tenantGroups.IsFailure)
                {
                    return Result.Failure(tenantGroups.Error);
                }
                var group = tenantGroups.Value.SubGroups.FirstOrDefault(g => g.Name == GroupName);

                if (group == null)
                {
                    return Result.Failure(Error.Failure("GroupNotFound", $"Group with name {GroupName} was not found for tenant with ID {TenantID} in Keycloak"));
                }

                await Client.Admin.Realms[Constants.REALM]
                                  .Users[UserID]
                                  .Groups[group.Id.ToString()]
                                  .DeleteAsync();
                return Result.Success();

            }
            catch (ApiException ex)
            {
                return ex.ResponseStatusCode switch
                {
                    404 => Result.Failure(Error.Failure("GroupNotFound", $"Group with name {GroupName} was not found for tenant with ID {TenantID} in Keycloak")),
                    400 => Result.Failure(Error.Failure("RemoveUserFromGroupFailed", $"Failed to remove user with ID {UserID} from group with Name {GroupName} in Keycloak. Possible reasons could be that the user is not a member of the group or the user or group does not exist.")),
                    _ => Result.Failure(Error.Failure("RemoveUserFromGroupFailed", $"Failed to remove user with ID {UserID} from group with Name {GroupName} in Keycloak. Error: {ex.Message}"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove user with ID {UserID} from group with {GroupName}. Error: {ErrorMessage}", UserID, GroupName, ex.Message);
                return Result.Failure(Error.Failure("RemoveUserFromGroupFailed", $"Failed to remove user with ID {UserID} from group with Name {GroupName} in Keycloak"));
            }
        }
    }
}
