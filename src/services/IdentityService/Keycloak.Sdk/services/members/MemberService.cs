using CCP.Sdk.utils.Abstractions;
using CCP.Shared.ResultAbstraction;
using Keycloak.Sdk.Models;
using Keycloak.Sdk.ServiceDefaults;
using Microsoft.Extensions.Logging;

namespace Keycloak.Sdk.services.members
{
    internal class MemberService : IMemberKeycloakService
    {
        private readonly ILogger<MemberService> _logger;
        private readonly IKiotaApiClient<KeycloakClient> _kiotaApiClient;

        private KeycloakClient Client => _kiotaApiClient.Client;

        public MemberService(ILogger<MemberService> logger, IKiotaApiClient<KeycloakClient> kiotaApiClient)
        {
            _logger = logger;
            _kiotaApiClient = kiotaApiClient;
        }

        public async Task<Result<List<KeycloakTenantMember>>> GetAllMembersOfOrganization(Guid OrgId, CancellationToken ct = default)
        {
            try
            {
                List<MemberRepresentation>? members = await Client.Admin.Realms[Constants.REALM]
                                                .Organizations[OrgId.ToString()]
                                                .Members
                                                .GetAsync(cancellationToken: ct);
                if (members == null)
                    return Result.Failure<List<KeycloakTenantMember>>(Error.Failure("GetAllMembersOfOrganization.nullResponse", "Received null response from Keycloak API while retrieving members of the organization."));

                var DetailsTasks = members.Select(GetMemberDetails);

                var DetailsResults = await Task.WhenAll(DetailsTasks);

                var failedResults = DetailsResults.Where(r => r.IsFailure).ToList();

                if (failedResults.Count != 0)
                {
                    foreach (var failed in failedResults)
                    {
                        _logger.LogError("Failed to retrieve member details. Error: {Error}", failed.Error);
                    }
                    return Result.Failure<List<KeycloakTenantMember>>(Error.Failure("GetAllMembersOfOrganization.memberDetailsFailed", "Failed to retrieve details for one or more members of the organization. Check logs for more details."));
                }

                return Result.Success(DetailsResults.Where(r => r.IsSuccess).Select(r => r.Value).ToList());

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving members of organization {OrganizationName}", OrgId);
                return Result.Failure<List<KeycloakTenantMember>>(Error.Failure("GetAllMembersOfOrganization.failed", $"An error occurred while retrieving members of the organization: {ex.Message}"));
            }
        }

        private async Task<Result<KeycloakTenantMember>> GetMemberDetails(MemberRepresentation member)
        {
            try
            {
                if (!Guid.TryParse(member.Id, out Guid userId))
                {
                    _logger.LogWarning("Invalid member ID format for member with UserId {UserId}. Received ID: {MemberId}", null, member.Id);
                    return Result.Failure<KeycloakTenantMember>(Error.Failure("GetMemberDetails.invalidIdFormat", $"The member ID retrieved for user '{member.Id}' is not in a valid GUID format."));
                }

                Result<List<KeycloakGroup>> GroupMemberships = await GetMemberGroupMemberships(userId);

                if (GroupMemberships.IsFailure)
                {
                    _logger.LogError("Failed to retrieve group memberships for member with UserId {UserId}. Error: {Error}", userId, GroupMemberships.Error);
                    return Result.Failure<KeycloakTenantMember>(Error.Failure("GetMemberDetails.groupMembershipsFailed", $"Failed to retrieve group memberships for the member. Error: {GroupMemberships.Error.Description}"));
                }

                List<string> groups = [.. GroupMemberships.Value.Where(g => !string.IsNullOrEmpty(g.Name)).Select(g => g.Name!)];
                var roles = GroupMemberships.Value.SelectMany(g => g.Roles).Distinct().ToList();

                KeycloakTenantMember tenantMember = new KeycloakTenantMember
                {
                    Id = userId,
                    FirstName = member.FirstName ?? string.Empty,
                    LastName = member.LastName ?? string.Empty,
                    Email = member.Email ?? string.Empty,
                    Groups = groups,
                    Roles = roles,
                };

                return Result.Success(tenantMember);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving details for member {UserId}", member.Id);
                return Result.Failure<KeycloakTenantMember>(Error.Failure("GetMemberDetails.failed", $"An error occurred while retrieving details for the member: {ex.Message}"));
            }
        }

        private async Task<Result<List<KeycloakGroup>>> GetMemberGroupMemberships(Guid userId, CancellationToken ct = default)
        {
            try
            {
                List<GroupRepresentation>? groups = await Client.Admin.Realms[Constants.REALM]
                                                .Users[userId.ToString()]
                                                .Groups
                                                .GetAsync(cancellationToken: ct);

                if (groups == null)
                    return Result.Failure<List<KeycloakGroup>>(Error.Failure("GetMemberGroupMemberships.nullResponse", "Received null response from Keycloak API while retrieving group memberships for the user."));


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

                return Result.Success(groups.Select(MapGroup).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving group memberships for user {UserId}", userId);
                return Result.Failure<List<KeycloakGroup>>(Error.Failure("GetMemberGroupMemberships.failed", $"An error occurred while retrieving group memberships for the user: {ex.Message}"));
            }
        }
    }
}
