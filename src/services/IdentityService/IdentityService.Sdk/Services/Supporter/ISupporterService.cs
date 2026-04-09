using System;
using System.Collections.Generic;
using System.Text;
using CCP.Shared.ResultAbstraction;
using IdentityService.Sdk.Models;

namespace IdentityService.Sdk.Services.Supporter
{
    //Interface til supporter operator i SDK laget
    public interface ISupporterService
    {
        Task<Result> InviteSupporter(string email, CancellationToken ct = default);
        Task<Result<List<TenantMember>>> GetAllSupporters(CancellationToken ct = default);
    }
}
