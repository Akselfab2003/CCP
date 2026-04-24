using CCP.Shared.AuthContext;
using CCP.Shared.ResultAbstraction;
using IdentityService.Application.Models;
using IdentityService.Application.Services.Group;
using IdentityService.Application.Services.Organization;
using IdentityService.Application.Services.User;
using Keycloak.Sdk.services.management;
using Keycloak.Sdk.services.members;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Services.Customer
{
    public class CustomerService : ICustomerService
    {
        private readonly ILogger<CustomerService> _logger;
        private readonly IOrganizationService _organizationService;
        private readonly ICurrentUser _currentUser;
        private readonly IGroupService _groupService;
        private readonly IUserService _userService;
        private readonly IManagementKeycloakService _managementService;
        private readonly IMemberKeycloakService _memberService;
        public CustomerService(ILogger<CustomerService> logger,
                               IOrganizationService organizationService,
                               ICurrentUser currentUser,
                               IGroupService groupService,
                               IUserService userService,
                               IManagementKeycloakService managementService,
                               IMemberKeycloakService memberService)
        {
            _logger = logger;
            _organizationService = organizationService;
            _currentUser = currentUser;
            _groupService = groupService;
            _userService = userService;
            _managementService = managementService;
            _memberService = memberService;
        }

        public async Task<Result<Guid>> InviteCustomer(string Email, CancellationToken ct = default)
        {
            try
            {
                var CreateUserResult = await _userService.CreateCustomer(Email, ct);
                if (CreateUserResult.IsFailure)
                {
                    _logger.LogWarning("Failed to create customer with email {Email}: {Error}", Email, CreateUserResult.Error.Description);
                    return Result.Failure<Guid>(CreateUserResult.Error);
                }

                var userId = CreateUserResult.Value;
                var JoinOrgResult = await _organizationService.InviteExistingUserToOrganization(_currentUser.OrganizationId, userId, ct);

                if (JoinOrgResult.IsFailure)
                {
                    _logger.LogWarning("Failed to invite customer with email {Email} to organization: {Error}", Email, JoinOrgResult.Error.Description);
                    return Result.Failure<Guid>(JoinOrgResult.Error);
                }

                var AddUserToGroupResult = await _groupService.AddUserToGroup("Customers", _currentUser.OrganizationId, userId, ct);
                if (AddUserToGroupResult.IsFailure)
                {
                    _logger.LogWarning("Failed to add customer with email {Email} to Customers group: {Error}", Email, AddUserToGroupResult.Error.Description);
                    return Result.Failure<Guid>(AddUserToGroupResult.Error);
                }

                int lifespan = 24 * 60 * 60; // 24 hours in seconds
                var SendRequiredActionsEmailResult = await _managementService.ExecuteEmailRequiredActions(email: Email,
                                                                                                          userId: userId.ToString(),
                                                                                                          actions: ["UPDATE_PASSWORD"],
                                                                                                          lifespan: lifespan,
                                                                                                          ct: ct);
                if (SendRequiredActionsEmailResult.IsFailure)
                {
                    _logger.LogWarning("Failed to send required actions email to customer with email {Email}: {Error}", Email, SendRequiredActionsEmailResult.Error.Description);
                    return Result.Failure<Guid>(SendRequiredActionsEmailResult.Error);
                }

                return Result.Success(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inviting customer with email {Email}", Email);
                return Result.Failure<Guid>(Error.Failure(code: "InviteCustomerFailed", description: $"An error occurred while inviting the customer with email {Email}."));
            }
        }

        public async Task<Result<List<TenantMemberDto>>> GetAllTenantCustomerUsers(CancellationToken ct = default)
        {
            try
            {
                Guid TenantId = _currentUser.OrganizationId;
                var membersResult = await _memberService.GetAllMembersOfOrganization(TenantId, ct);

                if (membersResult.IsFailure)
                {
                    _logger.LogWarning("Failed to retrieve tenant customer users for tenant {TenantId}: {Error}", TenantId, membersResult.Error.Description);
                    return Result.Failure<List<TenantMemberDto>>(membersResult.Error);
                }

                var tenantMembers = membersResult.Value
                                                 .Where(m => m.Groups.Contains("Customer"))
                                                 .Select(member => new TenantMemberDto
                                                 {
                                                     Id = member.Id,
                                                     FirstName = member.FirstName,
                                                     LastName = member.LastName,
                                                     Email = member.Email,
                                                     Roles = member.Roles,
                                                     Groups = member.Groups
                                                 }).ToList();

                return tenantMembers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tenant customer users");
                return Result.Failure<List<TenantMemberDto>>(Error.Failure(code: "GetTenantCustomerUsersFailed", description: "An error occurred while retrieving tenant customer users."));
            }
        }

    }
}
