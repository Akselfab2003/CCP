using CCP.Shared.ResultAbstraction;
using Keycloak.Sdk.Models;

namespace Keycloak.Sdk.services.users
{
    public interface IUserKeycloakService
    {
        Task<Result<Guid>> CreateCustomer(string email, CancellationToken ct = default);
        Task<Result<Guid>> CreateUser(string email, string firstName, string lastName, string? password = null, CancellationToken ct = default);
        Task<Result<UserKeycloakAccount>> GetUserDetailsByID(Guid UserID, CancellationToken ct = default);
        Task<Result<List<UserKeycloakAccount>>> SearchUsers(string searchTerm, CancellationToken ct);
    }
}
