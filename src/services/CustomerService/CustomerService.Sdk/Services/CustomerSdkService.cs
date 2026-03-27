using CCP.Sdk.utils.Abstractions;
using CustomerService.Sdk.Models;

namespace CustomerService.Sdk.Services
{
    // Client implementation using Kiota to communicate with Customer API
    internal class CustomerSdkService : ICustomerSdkService
    {
        private readonly IKiotaApiClient<CustomerServiceClient> _apiClient;

        public CustomerSdkService(IKiotaApiClient<CustomerServiceClient> client)
        {
            _apiClient = client;
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
            catch (Exception)
            {

                throw;
            }
        }


    }
}
