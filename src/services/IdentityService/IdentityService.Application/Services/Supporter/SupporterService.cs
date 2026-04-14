using CCP.Shared.AuthContext;
using CCP.Shared.ResultAbstraction;
using CCP.Shared.ValueObjects;
using IdentityService.Application.Services.Group;
using IdentityService.Application.Services.Organization;
using IdentityService.Application.Services.User;
using Keycloak.Sdk.services.management;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Services.Supporter
{
    //Service der håndterer supporter operationer (business logic)
    public class SupporterService : ISupporterService
    {
        private readonly ILogger<SupporterService> _logger;
        private readonly IUserService _userService;
        private readonly IGroupService _groupService;
        private readonly IOrganizationService _organizationService;
        private readonly ICurrentUser _currentUser;
        private readonly IManagementKeycloakService _managementService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SupporterService(
            ILogger<SupporterService> logger,
            IUserService userService,
            IGroupService groupService,
            ICurrentUser currentUser,
            IOrganizationService organizationService,
            IConfiguration configuration,
            IManagementKeycloakService managementService,
            IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _userService = userService;
            _groupService = groupService;
            _currentUser = currentUser;
            _organizationService = organizationService;
            _managementService = managementService;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<Result> InviteSupporter(string email, CancellationToken ct = default)
        {
            try
            {
                var CreateSupportUserResult = await _userService.CreateSupporter(email, ct);
                if (CreateSupportUserResult.IsFailure)
                {
                    _logger.LogWarning("Failed to create supporter user with email {Email}: {Error}", email, CreateSupportUserResult.Error);
                    return CreateSupportUserResult;
                }

                var UserId = CreateSupportUserResult.Value;
                var JoinOrganizationResult = await _organizationService.InviteExistingUserToOrganization(OrgId: _currentUser.OrganizationId,
                                                                                                         userId: UserId,
                                                                                                         ct: ct);
                if (JoinOrganizationResult.IsFailure)
                {
                    _logger.LogWarning("Failed to invite user {UserId} to organization {OrganizationId}: {Error}",
                        UserId, _currentUser.OrganizationId, JoinOrganizationResult.Error);
                    return CreateSupportUserResult;
                }

                var AddToSupportersGroupResult = await _groupService.AddUserToGroup(groupName: UserRole.Supporter.ToGroupName(),
                                                                                    OrgId: _currentUser.OrganizationId,
                                                                                    userID: UserId,
                                                                                    ct: ct);
                if (AddToSupportersGroupResult.IsFailure)
                {
                    _logger.LogWarning("Failed to add user {UserId} to Supporters group in organization {OrganizationId}: {Error}",
                        UserId, _currentUser.OrganizationId, AddToSupportersGroupResult.Error);
                    return CreateSupportUserResult;
                }

                int lifespan = 24 * 60 * 60; // 24 hours in seconds

                string RedirectUrl = string.Empty;
                if (_webHostEnvironment.IsDevelopment())
                {
                    RedirectUrl = "https://localhost:7033";
                }
                else
                {
                    RedirectUrl = _configuration["SupporterInvitationRedirectUrl"] ?? throw new InvalidOperationException("SupporterInvitationRedirectUrl is not configured");
                }

                var SendRequiredActionsEmailResult = await _managementService.ExecuteEmailRequiredActions(email: email,
                                                                                                          userId: UserId.ToString(),
                                                                                                          actions: ["UPDATE_PASSWORD"],
                                                                                                          redirectUrl: RedirectUrl,
                                                                                                          lifespan: lifespan,
                                                                                                          ct: ct);

                if (SendRequiredActionsEmailResult.IsFailure)
                {
                    _logger.LogWarning("Failed to send required actions email to user {UserId} with email {Email}: {Error}",
                        UserId, email, SendRequiredActionsEmailResult.Error);
                    return CreateSupportUserResult;
                }

                return Result.Success();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inviting supporter with email {Email}", email);
                return Result.Failure(Error.Failure(code: "InviteSupporterFailed",
                                                    description: "an error occurred while inviting supporter"));
            }
        }
    }
}
