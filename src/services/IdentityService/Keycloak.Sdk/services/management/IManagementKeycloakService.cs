using CCP.Shared.ResultAbstraction;

namespace Keycloak.Sdk.services.management
{
    public interface IManagementKeycloakService
    {
        Task<Result> ExecuteEmailRequiredActions(string email, string userId, List<string> actions, string redirectUrl = "https://localhost:7033", int lifespan = 300, CancellationToken ct = default);
    }
}
