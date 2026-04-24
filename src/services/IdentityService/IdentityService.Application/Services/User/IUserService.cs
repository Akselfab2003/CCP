using CCP.Shared.ResultAbstraction;
using IdentityService.Application.Models;
using Keycloak.Sdk.Models;

namespace IdentityService.Application.Services.User
{
    public interface IUserService
    {
        Task<Result<string>> Authenticate(AuthenticatingRequest authenticatingRequest, CancellationToken ct = default);
        Task<Result<Guid>> CreateCustomer(string email, CancellationToken ct = default);
        Task<Result<Guid>> CreateSupporter(string email, CancellationToken ct = default);
        Task<Result<Guid>> CreateUser(string email, string firstName, string lastName, string password, CancellationToken ct = default);
        Task<Result<UserKeycloakAccount>> GetUserDetails(Guid UserID, CancellationToken ct);
        Task<Result<List<UserKeycloakAccount>>> SearchUsers(string searchTerm, CancellationToken ct);
    }
}
