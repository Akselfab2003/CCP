using CustomerService.Domain.Entities;

namespace CustomerService.Application.Persistence
{
    public interface ICustomerRepository
    {
        Task<Customer> CreateCustomer(Customer customer);
        Task<bool> DeleteCustomer(Guid id);
        Task<List<Customer>> GetAllCustomersAsync();
        Task<Customer?> GetCustomerById(Guid id);
        Task<Customer?> GetCustomerByEmail(string email);
        Task<Customer?> UpdateCustomer(Guid id, Customer updatedCustomer);
    }
}
