using CCP.Shared.ResultAbstraction;
using Keycloak.Sdk.Models;

namespace Keycloak.Sdk.services.organizations
{
    public interface IOrganizationKeycloakService
    {
        Task<Result> AddExistingMemberToOrganizationAsync(Guid OrgId, Guid userId, CancellationToken ct = default);
        Task<Result<Guid>> CreateOrganizationAsync(string organizationName, string DomainName, CancellationToken ct = default);
        Task<Result<KeycloakOrgDetails>> GetOrgDetails(Guid? OrgId = null, string? Domain = null, CancellationToken ct = default);
        Task<Result> InviteMemberToOrg(Guid OrgId, string email, CancellationToken ct = default);
    }
}
