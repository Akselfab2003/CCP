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
                // 3. Send invitation email med link til at oprette konto med Supporter rolle
                // 4. Gem pending invitation i database
                var serchResult = await _userService.SearchUsers(email, ct);

                //returnere fejl, hvis der ikke kan validers
                if(serchResult.IsFailure)
                {
                    _logger.LogWarning("Failed to search for user with email {Email}: {Error}", email, serchResult.Error);
                    return Result.Failure(serchResult.Error);
                }

                //Checker efter samme email
                var existingUser = serchResult.Value.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

                if (existingUser != null)
                {
                    _logger.LogWarning("User with email {Email} already exists", email);
                    return Result.Failure(Error.Conflict(
                        code: "UserAlreadyExists",
                        description: $"A user with email {email} already exists."));
                }
                _logger.LogInformation("Email {Email} is valid and not in use, proceeding with invitation", email);

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
                _logger.LogInformation("🔍 Fetching all supporters for tenant {TenantId}", tenantId);

                //Hent alle medlemmer af organisationen
                var membersResult = await _memberService.GetAllMembersOfOrganization(tenantId, ct);

                if (membersResult.IsFailure)
                {
                    _logger.LogWarning("Failed to retrieve members for tenant {TenantId}: {Error}",
                        tenantId, membersResult.Error.Description);
                    return Result.Failure<List<TenantMemberDto>>(membersResult.Error);
                }

                _logger.LogInformation("📊 Retrieved {Count} total members", membersResult.Value?.Count ?? 0);

                //Filtrer på dem der er supporters
                var supporters = (membersResult.Value ?? new List<Keycloak.Sdk.Models.KeycloakTenantMember>())
                    .Where(member => member.Roles != null &&
                                   member.Roles.Any(role => role.Equals("org.Supporter", StringComparison.OrdinalIgnoreCase)))
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

                _logger.LogInformation("✅ Found {Count} supporters in tenant {TenantId}", supporters.Count, tenantId);

                // Log detaljer om hver medlem for debugging
                foreach (var member in (membersResult.Value ?? new List<Keycloak.Sdk.Models.KeycloakTenantMember>()).Take(5))
                {
                    _logger.LogInformation("  Member: {FirstName} {LastName} - Roles: {Roles}", 
                        member.FirstName, member.LastName, string.Join(", ", member.Roles ?? new List<string>()));
                }

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

        //Opgraderer en supporter til manager ved at flytte mellem grupper
        public async Task<Result> PromoteToManager(Guid supporterId, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInformation("Promoting supporter {SupporterId} to Manager", supporterId);

                Guid tenantId = _currentUser.OrganizationId;

                // Fjern fra Supporters gruppe
                var removeResult = await _groupService.RemoveUserFromGroup("Supporters", tenantId, supporterId, ct);
                if (removeResult.IsFailure)
                {
                    _logger.LogWarning("Failed to remove user {SupporterId} from Supporters group: {Error}", 
                        supporterId, removeResult.Error);
                    return Result.Failure(removeResult.Error);
                }

                // Tilføj til Managers gruppe
                var addResult = await _groupService.AddUserToGroup("Managers", tenantId, supporterId, ct);
                if (addResult.IsFailure)
                {
                    _logger.LogWarning("Failed to add user {SupporterId} to Managers group: {Error}", 
                        supporterId, addResult.Error);
                    return Result.Failure(addResult.Error);
                }

                _logger.LogInformation("Successfully promoted supporter {SupporterId} to Manager", supporterId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error promoting supporter {SupporterId} to Manager", supporterId);
                return Result.Failure(Error.Failure(
                    code: "PromoteToManagerFailed",
                    description: $"An error occurred while promoting supporter to manager: {ex.Message}"));
            }
        }
    }
}
