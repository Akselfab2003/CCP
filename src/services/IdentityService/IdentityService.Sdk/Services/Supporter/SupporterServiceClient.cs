using System;
using System.Collections.Generic;
using System.Text;
using CCP.Sdk.utils.Abstractions;
using CCP.Shared.ResultAbstraction;
using IdentityService.Sdk.Models;
using Microsoft.Extensions.Logging;

namespace IdentityService.Sdk.Services.Supporter
{
    //HTTP client der kalder IdentityService API'ets supporter endpoints
    internal class SupporterServiceClient : ISupporterService
    {
        private readonly ILogger<SupporterServiceClient> _logger;
        private readonly IKiotaApiClient<IdentityServiceClient> _client;

        public SupporterServiceClient(ILogger<SupporterServiceClient> logger,IKiotaApiClient<IdentityServiceClient> client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task<Result> InviteSupporter(Guid customerId, CancellationToken ct = default)
        {
            try
            {
                // Sender POST request til API'et
                await _client.Client.Supporter.Invite.PostAsync(req =>
                {
                    req.QueryParameters.CustomerId = customerId;
                }, cancellationToken: ct);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to invite supporter with customerId {CustomerId}", customerId);
                return Result.Failure(Error.Failure(
                    code: "InviteSupporterFailed",
                    description: $"Failed to invite supporter: {ex.Message}"));
            }
        }

        public async Task<Result<List<TenantMember>>> GetAllSupporters(CancellationToken ct = default)
        {
            try
            {
                // Sender GET request til API'et
                var supporters = await _client.Client.Supporter.GetAllSupporters.GetAsync(cancellationToken: ct);

                if (supporters == null)
                {
                    return Result.Failure<List<TenantMember>>(Error.NotFound(
                        code: "SupportersNotFound",
                        description: "No supporters found"));
                }

                // Konverter fra API model til SDK model
                var tenantMembers = supporters.Select(s => new TenantMember
                {
                    Id = s.Id ?? Guid.Empty,
                    FirstName = s.FirstName ?? string.Empty,
                    LastName = s.LastName ?? string.Empty,
                    Email = s.Email ?? string.Empty
                }).ToList();

                return Result.Success(tenantMembers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all supporters");
                return Result.Failure<List<TenantMember>>(Error.Failure(
                    code: "GetSupportersFailed",
                    description: $"Failed to get supporters: {ex.Message}"));
            }
        }
    }
}
