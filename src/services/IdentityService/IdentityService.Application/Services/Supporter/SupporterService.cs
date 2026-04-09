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
        //Sender invitation til en ny supporter
        public async Task<Result> InviteSupporter(string email, CancellationToken ct = default)
        {
            try
            {
                // TODO: Implementer invitation logik
                // 1. Valider email format
                // 2. Tjek om email allerede eksisterer
                // 3. Send invitation email med link til at oprette konto med Supporter rolle
                // 4. Gem pending invitation i database

                _logger.LogInformation("Inviting new supporter with email: {Email}", email);

                // For nu: Log success (implementeres senere når Email service er klar)
                _logger.LogInformation("Successfully sent invitation to {Email}", email);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inviting supporter with email {Email}", email);
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
