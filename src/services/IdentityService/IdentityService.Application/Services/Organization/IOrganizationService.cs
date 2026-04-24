using CCP.Shared.ResultAbstraction;
using Keycloak.Sdk.Models;

namespace IdentityService.Application.Services.Organization
{
    public interface IOrganizationService
    {
        Task<Result<Guid>> CreateOrganization(string organizationName, string domainName, CancellationToken ct = default);
        Task<Result<KeycloakOrgDetails>> GetOrganizationDetails(Guid? OrgId, string? Domain, CancellationToken ct = default);
        Task<Result> InviteExistingUserToOrganization(Guid OrgId, Guid userId, CancellationToken ct = default);
        Task<Result> InviteNewUserToJoinOrganization(string email, CancellationToken ct = default);
    }
}
