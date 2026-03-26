using CustomerService.Api.DB.Models;

namespace CustomerService.Application.Services
{
    public interface ICustomerService
    {
        Task<Customer> CreateCustomer(Customer customer);
        Task<bool> DeleteCustomer(Guid id);
        Task<List<Customer>> GetAllCustomers();
        Task<Customer?> GetCustomerById(Guid id);
        Task<Customer?> UpdateCustomer(Customer customer);
    }
}
