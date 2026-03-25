using System.Text.RegularExpressions;
using CCP.Shared.AuthContext;
using CCP.Shared.ResultAbstraction;
using Keycloak.Sdk.services.organizations;
using Microsoft.Extensions.Logging;

namespace IdentityService.Application.Services.Organization
{
    public class OrganizationService : IOrganizationService
    {
        private readonly ILogger<OrganizationService> _logger;
        private readonly IOrganizationKeycloakService _organizationKeycloakService;
        private readonly ICurrentUser _currentUser;
        public OrganizationService(ILogger<OrganizationService> logger, IOrganizationKeycloakService organizationKeycloakService, ICurrentUser currentUser)
        {
            _logger = logger;
            _organizationKeycloakService = organizationKeycloakService;
            _currentUser = currentUser;
        }

        private string CleanOrganizationName(string organizationName)
        {
            if (string.IsNullOrWhiteSpace(organizationName))
                return string.Empty;

            string cleaned = organizationName.ToLower().Trim();
            cleaned = Regex.Replace(cleaned, @"[ \-\.,;'""]", "_"); // Replace spaces and special characters with underscores
            cleaned = Regex.Replace(cleaned, @"_+", "_");
            return cleaned;
        }



        public async Task<Result<Guid>> CreateOrganization(string organizationName, string domainName, CancellationToken ct = default)
        {
            try
            {
                string CleanName = CleanOrganizationName(organizationName);
                var result = await _organizationKeycloakService.CreateOrganizationAsync(CleanName, domainName, ct);
                if (result.IsFailure)
                {
                    _logger.LogWarning("Failed to create organization with name: {OrganizationName}. Error: {Error}", organizationName, result.Error);
                    return Result.Failure<Guid>(result.Error);
                }
                _logger.LogInformation("Successfully created organization with name: {OrganizationName}", organizationName);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating organization with name: {OrganizationName}", organizationName);
                return Result.Failure<Guid>(Error.Failure("CreateOrganization.Error", $"An error occurred while creating the organization: {ex.Message}"));
            }
        }

        public async Task<Result> InviteExistingUserToOrganization(Guid OrgId, Guid userId, CancellationToken ct = default)
        {
            try
            {
                var result = await _organizationKeycloakService.AddExistingMemberToOrganizationAsync(OrgId, userId, ct);
                if (result.IsFailure)
                {
                    _logger.LogWarning("Failed to invite existing user with ID: {UserId} to organization with ID: {OrgId}. Error: {Error}", userId, OrgId, result.Error);
                    return Result.Failure(result.Error);
                }
                _logger.LogInformation("Successfully invited existing user with ID: {UserId} to organization with ID: {OrgId}", userId, OrgId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inviting existing user with ID: {UserId} to organization with ID: {OrgId}", userId, OrgId);
                return Result.Failure(Error.Failure("InviteExistingUser.Error", $"An error occurred while inviting the existing user to the organization: {ex.Message}"));
            }
        }

        public async Task<Result> InviteNewUserToJoinOrganization(string email, CancellationToken ct = default)
        {
            try
            {
                var org_id = _currentUser.OrganizationId;
                if (org_id == Guid.Empty)
                {
                    _logger.LogWarning("Current user does not belong to any organization. Cannot invite new user with email: {Email}", email);
                    return Result.Failure(Error.Failure("InviteNewUser.NoOrg", "Current user does not belong to any organization."));
                }

                var result = await _organizationKeycloakService.InviteMemberToOrg(org_id, email, ct);

                if (result.IsFailure)
                {
                    _logger.LogWarning("Failed to invite new user with email: {Email} to organization with ID: {OrgId}. Error: {Error}", email, org_id, result.Error);
                    return Result.Failure(result.Error);
                }

                _logger.LogInformation("Successfully invited new user with email: {Email} to organization with ID: {OrgId}", email, org_id);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inviting new user with email: {Email} to organization", email);
                return Result.Failure(Error.Failure("InviteNewUser.Error", $"An error occurred while inviting the new user to the organization: {ex.Message}"));
            }
        }
    }
}
