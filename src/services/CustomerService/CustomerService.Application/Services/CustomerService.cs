using CustomerService.Api.DB.Models;
using CustomerService.Application.Persistence;
using Microsoft.Extensions.Logging;

namespace CustomerService.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ILogger<CustomerService> _logger;
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ILogger<CustomerService> logger, ICustomerRepository customerRepository)
        {
            _logger = logger;
            _customerRepository = customerRepository;
        }

        public Task<List<Customer>> GetAllCustomers()
        {
            return _customerRepository.GetAllCustomersAsync();
        }

        public async Task<Customer?> GetCustomerById(Guid id)
        {
            var customer = await _customerRepository.GetCustomerById(id);
            if (customer == null)
            {
                _logger.LogWarning("Customer with ID {CustomerId} not found.", id);
            }
            return customer;
        }

        public Task<Customer> CreateCustomer(Customer customer)
        {
            return _customerRepository.CreateCustomer(customer);
        }

        public async Task<bool> DeleteCustomer(Guid id)
        {
            return await _customerRepository.DeleteCustomer(id);
        }
        public async Task<Customer?> UpdateCustomer(Customer customer)
        {
            return await _customerRepository.UpdateCustomer(customer.Id, customer);
        }
    }
}
