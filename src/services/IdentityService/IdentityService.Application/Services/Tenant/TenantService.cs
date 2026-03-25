using CCP.Shared.ResultAbstraction;
using IdentityService.Application.Models;
using IdentityService.Application.Services.Group;
using IdentityService.Application.Services.Organization;
using IdentityService.Application.Services.User;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Services.Tenant
{
    public class TenantService : ITenantService
    {
        private readonly ILogger<TenantService> _logger;
        private readonly IUserService _userService;
        private readonly IOrganizationService _organizationService;
        private readonly IGroupService _groupService;

        public TenantService(ILogger<TenantService> logger,
                             IUserService userService,
                             IOrganizationService organizationService,
                             IGroupService groupService)
        {
            _logger = logger;
            _userService = userService;
            _organizationService = organizationService;
            _groupService = groupService;
        }


        public async Task<Result> CreateTenant(CreateTenantRequest request)
        {
            try
            {
                // Create a new organization for the tenant
                var organizationResult = await _organizationService.CreateOrganization(request.OrganizationName, request.DomainName, CancellationToken.None);

                if (organizationResult.IsFailure)
                {
                    _logger.LogError("Failed to create organization: {Error}", organizationResult.Error);
                    return Result.Failure(Error.Failure(code: "OrganizationCreationFailed", description: "Failed to create organization for the tenant."));
                }

                Guid OrgID = organizationResult.Value;

                // Create Required Groups for the tenant
                var groupResult = await _groupService.CreateDefaultGroupsForOrganization(OrgID, CancellationToken.None);

                if (groupResult.IsFailure)
                {
                    _logger.LogError("Failed to create default groups: {Error}", groupResult.Error);
                    return Result.Failure(Error.Failure(code: "GroupCreationFailed", description: "Failed to create default groups for the tenant."));
                }


                var createAdminUserResult = await _userService.CreateUser(request.AdminUser.Email, request.AdminUser.FirstName, request.AdminUser.LastName, request.AdminUser.Password, ct: CancellationToken.None);
                if (createAdminUserResult.IsFailure)
                {
                    _logger.LogError("Failed to create admin user: {Error}", createAdminUserResult.Error);
                    return Result.Failure(Error.Failure(code: "AdminUserCreationFailed", description: "Failed to create admin user for the tenant."));
                }

                Guid AdminUserID = createAdminUserResult.Value;

                var addUserToOrganizationResult = await _organizationService.InviteExistingUserToOrganization(OrgId: OrgID,
                                                                                                              userId: AdminUserID,
                                                                                                              ct: CancellationToken.None);

                if (addUserToOrganizationResult.IsFailure)
                {
                    _logger.LogError($"Failed to add admin user to organization: {addUserToOrganizationResult.Error.Description}");
                    return Result.Failure(Error.Failure(code: "AddAdminToOrganizationFailed", description: "Failed to add admin user to the tenant organization."));
                }

                var addUserToAdminGroupResult = await _groupService.AddUserToGroup(GroupNames.Admins.ToString(), OrgID, AdminUserID, CancellationToken.None);
                if (addUserToAdminGroupResult.IsFailure)
                {
                    _logger.LogError($"Failed to add admin user to Admins group: {addUserToAdminGroupResult.Error.Description}");
                    return Result.Failure(Error.Failure(code: "AddAdminToGroupFailed", description: "Failed to add admin user to the Admins group."));
                }

                _logger.LogInformation("Tenant created successfully with Organization ID: {OrgID} and Admin User ID: {AdminUserID}", OrgID, AdminUserID);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tenant");
                return Result.Failure(Error.Failure(code: "TenantCreationFailed", description: "An error occurred while creating the tenant."));
            }
        }
    }
}
