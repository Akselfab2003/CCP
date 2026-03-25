using CCP.Sdk.utils.Abstractions;
using CCP.Shared.ResultAbstraction;
using IdentityService.Sdk.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;

namespace IdentityService.Sdk.Services.Customer
{
    internal class CustomerServiceClient : ICustomerService
    {
        private readonly ILogger<CustomerServiceClient> _logger;
        private readonly IKiotaApiClient<IdentityServiceClient> _client;

        public CustomerServiceClient(ILogger<CustomerServiceClient> logger, IKiotaApiClient<IdentityServiceClient> client)
        {
            _logger = logger;
            _client = client;
        }

        public async Task<Result> InviteCustomer(string Email, CancellationToken ct = default)
        {
            try
            {
                await _client.Client.Customer.Invite.PostAsync(req =>
                {
                    req.QueryParameters.Email = Email;
                }, cancellationToken: ct);

                return Result.Success();
            }
            catch (ApiException apiEx)
            {
                _logger.LogError(apiEx, "API error inviting customer with email: {Email}", Email);
                return Result.Failure(Error.Failure("InviteCustomer.ApiError", $"API error occurred while inviting the customer: {apiEx.Message}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inviting customer with email: {Email}", Email);
                return Result.Failure(Error.Failure("InviteCustomer.Error", $"An error occurred while inviting the customer: {ex.Message}"));
            }
        }

        public async Task<Result<List<TenantMember>>> GetAllCustomers(CancellationToken ct = default)
        {
            try
            {

                var customers = await _client.Client.Customer.GetAllCustomers.GetAsync(cancellationToken: ct);

                if (customers == null)
                {
                    _logger.LogWarning("GetAllCustomers returned null");
                    return Result.Failure<List<TenantMember>>(Error.Failure("GetAllCustomers.NullResponse", "Received null response when retrieving customers"));
                }

                var tenantMembers = customers.Select(m => new TenantMember
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
            catch (ApiException apiEx)
            {
                _logger.LogError(apiEx, "API error retrieving customers");
                return Result.Failure<List<TenantMember>>(Error.Failure("GetAllCustomers.ApiError", $"API error occurred while retrieving customers: {apiEx.Message}"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers");
                return Result.Failure<List<TenantMember>>(Error.Failure("GetAllCustomers.Error", $"An error occurred while retrieving customers: {ex.Message}"));
            }
        }
    }
}
