using CustomerService.Sdk.Models;

namespace CustomerService.Sdk.Services
{
    // Client interface for calling Customer API from external services
    public interface ICustomerSdkService
    {
        Task CreateCustomer(CreateCustomerRequest customerRequest);
    }
}
