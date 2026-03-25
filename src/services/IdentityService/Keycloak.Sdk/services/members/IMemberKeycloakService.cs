using CCP.Shared.ResultAbstraction;
using Keycloak.Sdk.Models;

namespace Keycloak.Sdk.services.members
{
    public interface IMemberKeycloakService
    {
        Task<Result<List<KeycloakTenantMember>>> GetAllMembersOfOrganization(Guid OrgId, CancellationToken ct = default);


    }
}
