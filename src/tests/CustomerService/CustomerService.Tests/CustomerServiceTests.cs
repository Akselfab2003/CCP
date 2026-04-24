using ChatApp.Encryption;
using CustomerService.Api.DB;
using CustomerService.Domain.Entities;
using CustomerService.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using CustomerSVG = CustomerService.Infrastructure.Persistence.Repositories.CustomerRepository;

namespace CustomerService.Tests
{
    public class CustomerServiceTests
    {
        //Laver en in-memory database for test
        private CustomerDBContext GetInMemoryDbContext()
        {
            IEncryptionService encryptionService = NSubstitute.Substitute.For<IEncryptionService>(); //Mocking af IEncryptionService
            var options = new DbContextOptionsBuilder<CustomerDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unik database for hver test
                .Options;
            return new CustomerDBContext(options, encryptionService);
        }

        [Fact]
        public async Task GetCustomerById_ReturnsCustomer_WhenCustomerExists()
        {
            //Arrange
            using var context = GetInMemoryDbContext(); //ny database
            var service = new CustomerSVG(NullLogger<CustomerRepository>.Instance, context); // Lav service med fake database

            //Test kunde gemmes i database
            var testCustomer = new Customer
            {
                Id = Guid.NewGuid(),
                OrganizationId = Guid.NewGuid(),
                Name = "Test Customer",
                Email = "Test@test.com",
            };
            context.Customers.Add(testCustomer); //tilføjer til databsen
            await context.SaveChangesAsync(TestContext.Current.CancellationToken); // Kan stoppes

            //Act
            var result = await service.GetCustomerById(testCustomer.Id); //henter kunde

            //Assert
            Assert.NotNull(result); //bliver ikke fundet
            Assert.Equal(testCustomer.Id, result!.Id); //id matcher
            Assert.Equal(testCustomer.Name, result.Name); //navn matcher
            Assert.Equal(testCustomer.Email, result.Email); //email matcher

        }

        [Fact]
        public async Task GetCustomerById_ReturnsNull_WhenCustomerDoesNotExist()
        {
            //Arrange
            using var context = GetInMemoryDbContext(); //tom database
            var service = new CustomerSVG(NullLogger<CustomerRepository>.Instance, context);
            var nonExistentId = Guid.NewGuid();

            //Act
            var result = await service.GetCustomerById(nonExistentId); //henter kunde ud fra id

            //Assert
            Assert.Null(result); //Forventer null fordi kunden ikke findes

        }

        [Fact]
        public async Task GetAllCustomers_ReturnsAllCustomers_WhenCustomersExist()
        {
            //Arrange
            using var context = GetInMemoryDbContext();
            var service = new CustomerSVG(NullLogger<CustomerRepository>.Instance, context);

            //Test kunder
            context.Customers.AddRange(
                new Customer
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = Guid.NewGuid(),
                    Name = "Test Customer 1",
                    Email = "test1@test.com"
                },
                new Customer
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = Guid.NewGuid(),
                    Name = "Test Customer 2",
                    Email = "test2@test.com"
                },
                new Customer
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = Guid.NewGuid(),
                    Name = "Test Customer 3",
                    Email = "test3@test.com"
                });
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            //Act
            var result = await service.GetAllCustomersAsync(); //henter alle kunder

            //Assert
            Assert.NotNull(result); //bliver ikke fundet
            Assert.Equal(3, result.Count); //forventer 3 kunder
            // Tjek at ALLE 3 navne er til stede (uanset rækkefølge):
            Assert.Contains(result, c => c.Name == "Test Customer 1");
            Assert.Contains(result, c => c.Name == "Test Customer 2");
            Assert.Contains(result, c => c.Name == "Test Customer 3");
        }

        [Fact]
        public async Task GetAllCustomers_ReturnsEmptyList_WhenNoCustomersExist()
        {
            //Arrange
            using var context = GetInMemoryDbContext(); //tom database
            var service = new CustomerSVG(NullLogger<CustomerRepository>.Instance, context);

            //Act
            var result = await service.GetAllCustomersAsync();

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task CreateCustomer_CreatesAndReturnsCustomer_WithGeneratedId()
        {
            //Arrange
            using var context = GetInMemoryDbContext();
            var service = new CustomerSVG(NullLogger<CustomerRepository>.Instance, context);

            var newCustomer = new Customer
            {
                Id = Guid.Empty,
                OrganizationId = Guid.NewGuid(),
                Name = "New Customer",
                Email = "new@customer.com"
            };

            //Act
            var result = await service.CreateCustomer(newCustomer);
            var savedCustomer = await service.GetCustomerById(result.Id); //henter kunden ud fra det generede id

            //Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal(newCustomer.Name, result.Name); //Tjek at navnet matcher
            Assert.Equal(newCustomer.Email, result.Email); //Tjek at email matcher
            Assert.NotNull(savedCustomer); //Tjek at kunden er gemt i database
        }

        [Fact]
        public async Task UpdateCustomer_UpdatesAndReturnsCustomer_WhenCustomerExists()
        {
            //Arrange
            using var context = GetInMemoryDbContext();
            var service = new CustomerSVG(NullLogger<CustomerRepository>.Instance, context);

            //original bruger
            var originalCustomer = new Customer
            {
                Id = Guid.NewGuid(),
                OrganizationId = Guid.NewGuid(),
                Name = "Original Customer",
                Email = "original@test.com",
            };
            context.Customers.Add(originalCustomer);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            //Updatet bruger
            var updatedCustomer = new Customer
            {
                Id = originalCustomer.Id,
                OrganizationId = originalCustomer.OrganizationId,
                Name = "Updated Customer",
                Email = "Updated@test.com",
            };

            //Act
            var result = await service.UpdateCustomer(originalCustomer.Id, updatedCustomer);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(originalCustomer.Id, result!.Id); //id skal være det samme
            Assert.Equal(updatedCustomer.Name, result.Name); //navn skal være opdateret
            Assert.Equal(updatedCustomer.Email, result.Email); //email skal være opdateret
        }

        [Fact]
        public async Task UpdateCustomer_ReturnsNull_WhenCustomerDoesNotExist()
        {
            //Arrange
            using var context = GetInMemoryDbContext();
            var service = new CustomerSVG(NullLogger<CustomerRepository>.Instance, context);

            var nonExistingCustomer = new Customer
            {
                Id = Guid.NewGuid(),
                OrganizationId = Guid.NewGuid(),
                Name = "Non-existing Customer",
                Email = "nonExisting@test.com",
            };

            //Act
            var result = await service.UpdateCustomer(nonExistingCustomer.Id, nonExistingCustomer);

            //Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteCustomer_ReturnsTrue_AndDeletesCustomer_WhenCustomerExists()
        {
            //Arrange
            using var context = GetInMemoryDbContext();
            var service = new CustomerSVG(NullLogger<CustomerRepository>.Instance, context);

            var newCustomer = new Customer
            {
                Id = Guid.NewGuid(),
                OrganizationId = Guid.NewGuid(),
                Name = "Customer to Delete",
                Email = "Customer@test.com",
            };
            context.Customers.Add(newCustomer); //tilføjer kunden til database
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            //Act
            var result = await service.DeleteCustomer(newCustomer.Id); //sletter kunden
            var deletedCustomer = await service.GetCustomerById(newCustomer.Id); //henter kunden ud fra id for at tjekke om den er slettet

            //Assert
            Assert.True(result); //forventer True fordi kunden blev slettet
            Assert.Null(deletedCustomer); //forventer null fordi kunden er slettet
        }

        [Fact]
        public async Task DeleteCustomer_ReturnsFalse_WhenCustomerDoesNotExist()
        {
            //Arrange
            using var context = GetInMemoryDbContext();
            var service = new CustomerSVG(NullLogger<CustomerRepository>.Instance, context);
            var nonExistingCustomerId = Guid.NewGuid();

            //Act
            var result = await service.DeleteCustomer(nonExistingCustomerId); //forsøger at slette en kunde der ikke findes

            //Assert
            Assert.False(result); //forventer False fordi kunden ikke findes
        }
    }
}
