using CCP.Shared.AuthContext;
using CCP.Shared.ResultAbstraction;
using IdentityService.Application.Models;
using IdentityService.Application.Services.Group;
using IdentityService.Application.Services.User;
using Keycloak.Sdk.services.members;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Services.Supporter
{

    //Service der håndterer supporter operationer (business logic)
    public class SupporterService : ISupporterService
    {
        private readonly ILogger<SupporterService> _logger;
        private readonly IUserService _userService;
        private readonly IGroupService _groupService;
        private readonly ICurrentUser _currentUser;
        private readonly IMemberKeycloakService _memberService;

        public SupporterService(
            ILogger<SupporterService> logger,
            IUserService userService,
            IGroupService groupService,
            ICurrentUser currentUser,
            IMemberKeycloakService memberService)
        {
            _logger = logger;
            _userService = userService;
            _groupService = groupService;
            _currentUser = currentUser;
            _memberService = memberService;
        }
      
        //Promoverer en eksisterende customer til supporter
        public async Task<Result> InviteSupporter(Guid customerId, CancellationToken ct = default)
        {
            try
            {
                //Tjek om customer eksisterer
                var getUserResult = await _userService.GetUserDetails(customerId, ct);
                if (getUserResult.IsFailure)
                {
                    _logger.LogWarning("Customer with ID {CustomerId} not found", customerId);
                    return Result.Failure(Error.NotFound(
                        code: "CustomerNotFound",
                        description: $"Customer with ID {customerId} does not exist"));
                }

                var user = getUserResult.Value;
                _logger.LogInformation("Found customer: {Email}", user.Email);

                //Tilføj customer til Supporters gruppe
                var addToGroupResult = await _groupService.AddUserToGroup(
                    groupName: "Supporters",
                    OrgId: _currentUser.OrganizationId,
                    userID: customerId,
                    ct: ct);

                if (addToGroupResult.IsFailure)
                {
                    _logger.LogError("Failed to add customer {CustomerId} to Supporters group: {Error}",
                        customerId, addToGroupResult.Error.Description);
                    return Result.Failure(addToGroupResult.Error);
                }

                _logger.LogInformation("Successfully promoted customer {CustomerId} to Supporter", customerId);

                //TODO (Optional): Send email til customer om deres nye rolle
                //await _emailService.SendRoleChangeNotification(user.email, "Supporter");

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inviting supporter with customerId {CustomerId}", customerId);
                return Result.Failure(Error.Failure(
                    code: "InviteSupporterFailed",
                    description: $"An error occurred while inviting supporter: {ex.Message}"));
            }
        }

        //Henter alle supporters i den nuværende organisation
        public async Task<Result<List<TenantMemberDto>>> GetAllTenantSupporterUsers(CancellationToken ct = default)
        {
            try
            {
                Guid tenantId = _currentUser.OrganizationId;

                //Hent alle medlemmer af organisationen
                var membersResult = await _memberService.GetAllMembersOfOrganization(tenantId, ct);

                if (membersResult.IsFailure)
                {
                    _logger.LogWarning("Failed to retrieve members for tenant {TenantId}: {Error}",
                        tenantId, membersResult.Error.Description);
                    return Result.Failure<List<TenantMemberDto>>(membersResult.Error);
                }

                //Filtrer på dem der er supporters
                var supporters = membersResult.Value
                    .Where(member => member.Roles != null &&
                                   member.Roles.Contains("Supporter", StringComparer.OrdinalIgnoreCase))
                    .Select(member => new TenantMemberDto
                    {
                        Id = member.Id,
                        Email = member.Email,
                        FirstName = member.FirstName,
                        LastName = member.LastName,
                        Roles = member.Roles,
                        Groups = member.Groups,
                    })
                    .ToList();

                _logger.LogInformation("Found {Count} supporters in tenant {TenantId}", supporters.Count, tenantId);

                return Result.Success(supporters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supporters for tenant {TenantId}", _currentUser.OrganizationId);
                return Result.Failure<List<TenantMemberDto>>(Error.Failure(
                    code: "GetSupportersFailed",
                    description: $"An error occurred while retrieving supporters: {ex.Message}"));
            }
        }
    }
}
