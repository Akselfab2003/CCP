using CCP.Shared.ResultAbstraction;
using CustomerService.Sdk.Models;

namespace CustomerService.Sdk.Services
{
    // Client interface for calling Customer API from external services
    public interface ICustomerSdkService
    {
        Task CreateCustomer(CreateCustomerRequest customerRequest);
        Task<List<CustomerDTO>> GetAllCustomers();
        Task<Result<CustomerDTO>> GetCustomerById(Guid id);
        Task<Result<CustomerDTO>> GetCustomerByEmail(string email);
    }
}
