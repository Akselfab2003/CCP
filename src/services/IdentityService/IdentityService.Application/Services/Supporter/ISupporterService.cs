using CCP.Shared.ResultAbstraction;

namespace IdentityService.Application.Services.Supporter
{
    // Interface til supporter business logic
    public interface ISupporterService
    {
        Task<Result> InviteSupporter(string email, CancellationToken ct = default);
    }
}
