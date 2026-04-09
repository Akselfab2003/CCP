using System;
using System.Collections.Generic;
using System.Text;
using CCP.Shared.ResultAbstraction;
using IdentityService.Application.Models;

namespace IdentityService.Application.Services.Supporter
{
    // Interface til supporter business logic
    public interface ISupporterService
    {
        Task<Result> InviteSupporter(string email, CancellationToken ct = default);
        Task<Result<List<TenantMemberDto>>> GetAllTenantSupporterUsers(CancellationToken ct = default);
    }
}
