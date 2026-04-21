using CustomerService.Api.DB;
using CustomerService.Api.DB.Models;
using CustomerService.Application.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CustomerService.Infrastructure.Persistence.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ILogger<CustomerRepository> _logger;
        private readonly CustomerDBContext _dbContext;

        public CustomerRepository(ILogger<CustomerRepository> logger, CustomerDBContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            return await _dbContext.Customers.ToListAsync();
        }

        public async Task<Customer?> GetCustomerById(Guid id)
        {
            return await _dbContext.Customers.FindAsync(id);
        }

        public async Task<Customer?> GetCustomerByEmail(string email)
        {
            return await _dbContext.Customers.FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<Customer> CreateCustomer(Customer customer)
        {
            // Generer en ny GUID for kunden
            customer.Id = customer.Id;
            // Tilføj kunden til databasen
            _dbContext.Customers.Add(customer);
            //Gem ændringerne
            await _dbContext.SaveChangesAsync();
            // Returner den oprettede kunde
            return customer;
        }

        public async Task<bool> DeleteCustomer(Guid id)
        {
            // Find kunden i databasen
            var customer = await _dbContext.Customers.FindAsync(id);

            // Hvis kunden ikke findes, returner false
            if (customer == null)
            {
                return false;
            }

            // Fjern kunden fra databasen
            _dbContext.Customers.Remove(customer);

            // Gem ændringerne
            await _dbContext.SaveChangesAsync();

            // Returner true for success
            return true;
        }

        public async Task<Customer?> UpdateCustomer(Guid id, Customer updatedCustomer)
        {
            // Find kunden i databasen
            var existingCustomer = await _dbContext.Customers.FindAsync(id);
            // Hvis kunden ikke findes, returner null
            if (existingCustomer == null)
            {
                return null;
            }

            // Opdater customer properties
            existingCustomer.Name = updatedCustomer.Name;
            existingCustomer.Email = updatedCustomer.Email;
            existingCustomer.OrganizationId = updatedCustomer.OrganizationId;

            await _dbContext.SaveChangesAsync();
            return existingCustomer;
        }
    }
}
