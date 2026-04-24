using CCP.Shared.AuthContext;
using CCP.Shared.ResultAbstraction;
using CCP.Shared.ValueObjects;
using IdentityService.Application.Models;
using Keycloak.Sdk.services.members;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Services.Member
{
    public class MemberService : IMemberService
    {
        private readonly ILogger<MemberService> _logger;
        private readonly IMemberKeycloakService _memberKeycloakService;
        private readonly ICurrentUser _currentUser;
        public MemberService(ILogger<MemberService> logger, IMemberKeycloakService memberKeycloakService, ICurrentUser currentUser)
        {
            _logger = logger;
            _memberKeycloakService = memberKeycloakService;
            _currentUser = currentUser;
        }

        private async Task<Result<List<TenantMemberDto>>> GetAllTenantMembers(List<string> groups, CancellationToken ct = default)
        {
            try
            {
                var orgId = _currentUser.OrganizationId;

                if (orgId == Guid.Empty)
                {
                    _logger.LogWarning("Current user does not have an organization ID.");
                    return Result.Failure<List<TenantMemberDto>>(Error.Failure(code: "GetTenantMembersNoOrgId", description: "Current user does not have an organization ID."));
                }

                var membersResult = await _memberKeycloakService.GetAllMembersOfOrganization(orgId, CancellationToken.None);
                if (membersResult.IsFailure)
                {
                    _logger.LogError("Failed to retrieve tenant members: {Error}", membersResult.Error);
                    return Result.Failure<List<TenantMemberDto>>(Error.Failure(code: "GetTenantMembersFailed", description: "Failed to retrieve tenant members."));
                }

                var tenantMembers = membersResult.Value.Select(m => new TenantMemberDto
                {
                    Id = m.Id,
                    Email = m.Email,
                    FirstName = m.FirstName,
                    Groups = m.Groups,
                    LastName = m.LastName,
                    Roles = m.Roles,
                }).ToList();

                var filteredMembers = tenantMembers.Where(m => m.Groups != null && m.Groups.Intersect(groups).Any()).ToList();

                return Result.Success(filteredMembers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tenant members");
                return Result.Failure<List<TenantMemberDto>>(Error.Failure(code: "GetTenantMembersFailed", description: "An error occurred while retrieving tenant members."));
            }
        }

        /// <summary>
        /// Gets all internal users of the Tenant: Admins, Managers and Supporters
        /// </summary>
        /// <returns>
        /// A list of TenantMemberDto representing the internal users of the tenant, or an error if the operation fails.
        /// </returns>
        public async Task<Result<List<TenantMemberDto>>> GetAllInternalUsers()
        {
            try
            {
                return await GetAllTenantMembers(
                [
                    UserRole.Admin.ToGroupName(),
                    UserRole.Manager.ToGroupName(),
                    UserRole.Supporter.ToGroupName()
                ]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving internal users of tenant");
                return Result.Failure<List<TenantMemberDto>>(Error.Failure(code: "GetInternalUsersFailed", description: "An error occurred while retrieving internal users of tenant."));
            }
        }


        public async Task<Result<List<TenantMemberDto>>> GetAllSupportUsersOfTenant()
        {
            try
            {
                return await GetAllTenantMembers([UserRole.Supporter.ToGroupName()]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supporter users of tenant");
                return Result.Failure<List<TenantMemberDto>>(Error.Failure(code: "GetSupporterUsersFailed", description: "An error occurred while retrieving supporter users of tenant."));
            }
        }
        public async Task<Result<List<TenantMemberDto>>> GetAllCustomerUsersOfTenant()
        {
            try
            {
                return await GetAllTenantMembers([UserRole.Customer.ToGroupName()]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer users of tenant");
                return Result.Failure<List<TenantMemberDto>>(Error.Failure(code: "GetCustomerUsersFailed", description: "An error occurred while retrieving customer users of tenant."));
            }
        }


        public async Task<Result<List<TenantMemberDto>>> GetAllAdminUsersOfTenant()
        {
            try
            {
                return await GetAllTenantMembers([UserRole.Admin.ToGroupName()]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving admin users of tenant");
                return Result.Failure<List<TenantMemberDto>>(Error.Failure(code: "GetAdminUsersFailed", description: "An error occurred while retrieving admin users of tenant."));
            }
        }


        public async Task<Result<List<TenantMemberDto>>> GetAllManagerUsersOfTenant()
        {
            try
            {
                return await GetAllTenantMembers([UserRole.Manager.ToGroupName()]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving manager users of tenant");
                return Result.Failure<List<TenantMemberDto>>(Error.Failure(code: "GetManagerUsersFailed", description: "An error occurred while retrieving manager users of tenant."));
            }
        }
    }
}
