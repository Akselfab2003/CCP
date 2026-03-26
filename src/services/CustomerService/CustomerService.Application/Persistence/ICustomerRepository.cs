using CustomerService.Api.DB.Models;

namespace CustomerService.Application.Persistence
{
    public interface ICustomerRepository
    {
        Task<Customer> CreateCustomer(Customer customer);
        Task<bool> DeleteCustomer(Guid id);
        Task<List<Customer>> GetAllCustomersAsync();
        Task<Customer?> GetCustomerById(Guid id);
        Task<Customer?> UpdateCustomer(Guid id, Customer updatedCustomer);
    }
}
