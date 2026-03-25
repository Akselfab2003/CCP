using CCP.Shared.AuthContext;
using CCP.Shared.ResultAbstraction;
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

        public async Task<Result<List<TenantMemberDto>>> GetAllTenantMembers()
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

                var OnlyTenantUsers = tenantMembers.Where(m => !m.Groups.Contains("Customers")).ToList();

                return Result.Success(OnlyTenantUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tenant members");
                return Result.Failure<List<TenantMemberDto>>(Error.Failure(code: "GetTenantMembersFailed", description: "An error occurred while retrieving tenant members."));
            }
        }
    }
}
