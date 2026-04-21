using CCP.Sdk.utils.Abstractions;
using CCP.Shared.ResultAbstraction;
using IdentityService.Sdk.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;

namespace IdentityService.Sdk.Services.Tenant
{
    internal class TenantServiceClient : ITenantService
    {
        private readonly ILogger<TenantServiceClient> _logger;
        private readonly IKiotaApiClient<IdentityServiceClient> _apiClient;

        public TenantServiceClient(ILogger<TenantServiceClient> logger, IKiotaApiClient<IdentityServiceClient> apiClient)
        {
            _logger = logger;
            _apiClient = apiClient;
        }

        public async Task<Result> CreateTenant(CreateTenantDTO createTenant, CancellationToken ct = default)
        {
            try
            {
                var tenantRequest = new CreateTenantRequest
                {
                    AdminUser = new CreateAdminUserRequest
                    {
                        FirstName = createTenant.AdminUser.FirstName,
                        LastName = createTenant.AdminUser.LastName,
                        Email = createTenant.AdminUser.Email,
                        Password = createTenant.AdminUser.Password
                    },
                    DomainName = createTenant.DomainName,
                    OrganizationName = createTenant.OrganizationName,
                };

                await _apiClient.Client.Tenant.Create.PostAsync(tenantRequest, cancellationToken: ct);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating tenant for organization '{OrganizationName}'", createTenant.OrganizationName);
                return Result.Failure(Error.Failure(code: "TenantCreationFailed", description: $"An error occurred while creating tenant for organization '{createTenant.OrganizationName}'"));
            }
        }


        public async Task<Result<List<TenantMember>>> GetAllTenantMemberAsync(CancellationToken ct = default)
        {
            try
            {
                List<TenantMemberDto>? members = await _apiClient.Client.Tenant.Members.GetAsync(cancellationToken: ct);
                if (members == null)
                {
                    _logger.LogWarning("No members found for the tenant.");
                    return Result.Failure<List<TenantMember>>(Error.Failure(code: "NoTenantMembers", description: "No members found for the tenant."));
                }

                var tenantMembers = members.Select(m => new TenantMember
                {
                    Id = m.Id.HasValue ? m.Id.Value : Guid.Empty,
                    Email = m.Email ?? string.Empty,
                    FirstName = m.FirstName ?? string.Empty,
                    Groups = m.Groups ?? [],
                    LastName = m.LastName ?? string.Empty,
                    Roles = m.Roles ?? [],
                }).ToList();

                return Result.Success(tenantMembers);
            }
            catch (ApiException ex)
            {
                return Result.Failure<List<TenantMember>>(Error.Failure(code: "GetTenantMembersApiError", description: $"API error occurred while retrieving tenant members: {ex.Message}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving tenant members");
                return Result.Failure<List<TenantMember>>(Error.Failure(code: "GetTenantMembersFailed", description: "An error occurred while retrieving tenant members."));
            }
        }


        public async Task<Result> InviteNewTenantMember(string email, CancellationToken ct = default)
        {
            try
            {
                await _apiClient.Client.Tenant.Invite.PostAsync(req =>
                {
                    req.QueryParameters.Email = email;
                }, cancellationToken: ct);

                return Result.Success();
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "API error occurred while inviting a new tenant member with email '{Email}'", email);
                return Result.Failure(Error.Failure(code: "InviteTenantMemberApiError", description: $"API error occurred while inviting a new tenant member with email '{email}': {ex.Message}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while inviting a new tenant member with email '{Email}'", email);
                return Result.Failure(Error.Failure(code: "InviteTenantMemberFailed", description: $"An error occurred while inviting a new tenant member with email '{email}'"));
            }
        }

        public async Task<Result<TenantDetails>> GetTenantDetailsAsync(Guid? tenantId = null, string? domain = null, CancellationToken ct = default)
        {
            try
            {
                var tenantDetailsDto = await _apiClient.Client.Tenant.Info.GetAsync(req =>
                {
                    if (tenantId.HasValue)
                    {
                        req.QueryParameters.TenantId = tenantId.Value;
                    }
                    if (!string.IsNullOrEmpty(domain))
                    {
                        req.QueryParameters.Domain = domain;
                    }

                }, cancellationToken: ct);
                if (tenantDetailsDto == null)
                {
                    _logger.LogWarning("Tenant details not found for TenantId: '{TenantId}' or Domain: '{Domain}'", tenantId, domain);
                    return Result.Failure<TenantDetails>(Error.Failure(code: "TenantDetailsNotFound", description: $"Tenant details not found for TenantId: '{tenantId}' or Domain: '{domain}'"));
                }
                var tenantDetails = new TenantDetails
                {
                    OrgId = tenantDetailsDto.TenantId != null ? tenantDetailsDto.TenantId.Value : Guid.Empty,
                    Name = tenantDetailsDto.OrgName ?? string.Empty,
                    DomainName = tenantDetailsDto.DomainName ?? string.Empty,
                };
                return Result.Success(tenantDetails);
            }
            catch (ApiException ex)
            {
                _logger.LogError(ex, "API error occurred while retrieving tenant details for TenantId: '{TenantId}' or Domain: '{Domain}'", tenantId, domain);
                return Result.Failure<TenantDetails>(Error.Failure(code: "GetTenantDetailsApiError", description: $"API error occurred while retrieving tenant details for TenantId: '{tenantId}' or Domain: '{domain}': {ex.Message}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving tenant details for TenantId: '{TenantId}' or Domain: '{Domain}'", tenantId, domain);
                return Result.Failure<TenantDetails>(Error.Failure(code: "GetTenantDetailsFailed", description: $"An error occurred while retrieving tenant details for TenantId: '{tenantId}' or Domain: '{domain}'"));
            }
        }
    }
}
