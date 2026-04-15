using CCP.Sdk.utils.Abstractions;
using CCP.Shared.ResultAbstraction;
using CustomerService.Sdk.Models;
using Microsoft.Extensions.Logging;

namespace CustomerService.Sdk.Services
{
    // Client implementation using Kiota to communicate with Customer API
    internal class CustomerSdkService : ICustomerSdkService
    {
        private readonly ILogger<CustomerSdkService> _logger;
        private readonly IKiotaApiClient<CustomerServiceClient> _apiClient;

        public CustomerSdkService(IKiotaApiClient<CustomerServiceClient> client, ILogger<CustomerSdkService> logger)
        {
            _apiClient = client;
            _logger = logger;
        }

        public async Task CreateCustomer(CreateCustomerRequest customerRequest)
        {
            try
            // Send POST request til Customer API
            {
                await _apiClient.Client.Api.Customers.PostAsync(new Customer()
                {
                    Name = customerRequest.Name,
                    Email = customerRequest.Email,
                    Id = customerRequest.Id,
                    OrganizationId = customerRequest.OrganizationId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer with id {CustomerId}", customerRequest.Id);
            }
        }

        public async Task<Result<CustomerDTO>> GetCustomerById(Guid id)
        {
            try
            {
                var response = await _apiClient.Client.Api.Customers[id].GetAsync();

                if (response != null)
                {
                    return Result.Success(new CustomerDTO() { Email = response.Email!, Id = response.Id!.Value, Name = response.Name!, OrganizationId = response.OrganizationId!.Value });

                }
                else
                {
                    return Result.Failure<CustomerDTO>(Error.NotFound("CustomerNotFound", $"Customer with id {id} was not found."));
                }

            }
            catch (Exception)
            {
                return Result.Failure<CustomerDTO>(Error.None);
            }
        }

        public async Task<List<CustomerDTO>> GetAllCustomers()
        {
            try
            {
                var response = await _apiClient.Client.Api.Customers.GetAsync();
                if (response != null)
                {
                    return response.Select(c => new CustomerDTO() { Email = c.Email!, Id = c.Id!.Value, Name = c.Name!, OrganizationId = c.OrganizationId!.Value }).ToList();
                }
                else
                {
                    return new List<CustomerDTO>();
                }
            }
            catch (Exception)
            {
                return new List<CustomerDTO>();
            }
        }
    }
}
