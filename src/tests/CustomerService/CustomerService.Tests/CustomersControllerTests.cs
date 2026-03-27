using CustomerService.Api.Controllers;
using CustomerService.Api.DB.Models;
using CustomerService.Application.Services;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace CustomerService.Tests
{
    public class CustomersControllerTests
    {
        private readonly ICustomerService _customerService;
        private readonly CustomersController _controller;

        public CustomersControllerTests()
        {
            _customerService = Substitute.For<ICustomerService>();
            _controller = new CustomersController(_customerService);
        }

        #region GetAllCustomers Tests

        [Fact]
        public async Task GetAllCustomers_ReturnsOkResult_WithListOfCustomers()
        {
            // Arrange - Forbered test data
            var testCustomers = new List<Customer>
            {
                new Customer
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = Guid.NewGuid(),
                    Name = "John Doe",
                    Email = "John@Doe.com"
                },
                new Customer
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = Guid.NewGuid(),
                    Name = "Jane Doe",
                    Email = "Jane@Doe.com"
                }
            };
            //Fortæller mockede service at den skal returnere data
            _customerService.GetAllCustomers().Returns(testCustomers);

            // Act - kald metoden vi tester
            var result = await _controller.GetAllCustomers();

            // Assert - Tjek at vi fik HTTP 200 OK
            var okResult = Assert.IsType<OkObjectResult>(result);

            //Tjek at data er vores liste
            var returnCustomers = Assert.IsType<List<Customer>>(okResult.Value);

            //Tjek at vi fik 2 kunder
            Assert.Equal(2, returnCustomers.Count);

            //Verficer at servicen blev kaldt
            await _customerService.Received(1).GetAllCustomers();
        }

        [Fact]
        public async Task GetAllCustomers_ReturnsOkResult_WithEmptyList_WhenNoCustomers()
        {
            // Arrange - Tom liste, simulere ingen kunder i databasen
            var emptyList = new List<Customer>();
            _customerService.GetAllCustomers().Returns(emptyList);

            // Act - kald metoden
            var result = await _controller.GetAllCustomers();

            // Assert - Tjek at vi fik HTTP 200 OK
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnCustomers = Assert.IsType<List<Customer>>(okResult.Value);

            //Tjek at listen er tom
            Assert.Empty(returnCustomers);
        }

        #endregion

        #region GetCustomerById Tests

        [Fact]
        public async Task GetCustomerById_ReturnsOkResult_WhenCustomerExists()
        {
            //Arrange - Lav test kunde medm specifikt id
            var customerId = Guid.NewGuid();
            var testCustomer = new Customer
            {
                Id = customerId,
                OrganizationId = Guid.NewGuid(),
                Name = "Test kunde",
                Email = "Test@gmail.com"
            };

            //fortæl mocken at den skal returnere denne kunde når den bliver kaldt med det specifikke id
            _customerService.GetCustomerById(customerId).Returns(testCustomer);

            //Act - kald metoden med ID
            var result = await _controller.GetCustomerById(customerId);

            //Assert - Tjek om HTTP 200 OK
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnCustomer = Assert.IsType<Customer>(okResult.Value);

            //Tjek at det er den rigtige kunde
            Assert.Equal(customerId, returnCustomer.Id);
            Assert.Equal("Test kunde", returnCustomer.Name);
            Assert.Equal("Test@gmail.com", returnCustomer.Email);

            //Verficer at servicen blev kaldt med det rigtige id
            await _customerService.Received(1).GetCustomerById(customerId);
        }

        [Fact]
        public async Task GetCustomerById_ReturnsNotFound_WhenCustomerDoesNotExist()
        {
            //Arrange - ID som ikke findes
            var nonExistentId = Guid.NewGuid();

            //fortæl mocken at den skal returnere NUll (ingen kunde fundet)
            _customerService.GetCustomerById(nonExistentId).Returns((Customer?)null);

            //Act - kald metoden med det ikke eksisterende ID
            var result = await _controller.GetCustomerById(nonExistentId);

            //Assert - Tjek at vi fik HTTP 404 Not Found
            Assert.IsType<NotFoundResult>(result);

            //Verficer at servicen blev kaldt med det rigtige id
            await _customerService.Received(1).GetCustomerById(nonExistentId);
        }

        #endregion

        #region CreateCustomer Tests

        [Fact]
        public async Task CreateCustomer_ReturnsCreatedAtActionResult_WithNewCustomer()
        {
            // Arrange: Lav en ny kunde (uden ID endnu)
            var newCustomer = new Customer
            {
                Id = Guid.Empty, // ID vil blive sat af servicen
                OrganizationId = Guid.NewGuid(),
                Name = "New Customer",
                Email = "new@customer.com"
            };

            // Simuler hvad servicen returnerer (med nyt ID)
            var createdCustomer = new Customer
            {
                Id = Guid.NewGuid(), // Servicen har sat et nyt ID
                OrganizationId = newCustomer.OrganizationId,
                Name = newCustomer.Name,
                Email = newCustomer.Email
            };

            // Fortæl mocken at returnere den "oprettede" kunde
            _customerService.CreateCustomer(newCustomer).Returns(createdCustomer);

            // Act: Opret kunden
            var result = await _controller.CreateCustomer(newCustomer);

            // Assert: Tjek at vi fik 201 Created
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);

            // Tjek at response indeholder den oprettede kunde
            var returnedCustomer = Assert.IsType<Customer>(createdAtActionResult.Value);
            Assert.Equal(createdCustomer.Id, returnedCustomer.Id);
            Assert.Equal("New Customer", returnedCustomer.Name);

            // Verificer at Location header peger på GetCustomerById
            Assert.Equal(nameof(CustomersController.GetCustomerById), createdAtActionResult.ActionName);
            Assert.Equal(createdCustomer.Id, createdAtActionResult.RouteValues?["id"]);

            // Tjek at servicen blev kaldt
            await _customerService.Received(1).CreateCustomer(newCustomer);
        }

        #endregion

        #region UpdateCustomer Tests
        [Fact]
        public async Task UpdateCustomer_ReturnsOkResult_WithUpdatedCustomer_WhenCustomerExists()
        {
            // Arrange: Setup eksisterende kunde og opdateret data
            var customerId = Guid.NewGuid();
            var updatedCustomer = new Customer
            {
                Id = customerId,
                OrganizationId = Guid.NewGuid(),
                Name = "Updated Name",
                Email = "updated@email.com"
            };

            // Mock servicen til at returnere den opdaterede kunde
            _customerService.UpdateCustomer(updatedCustomer).Returns(updatedCustomer);

            // Act: Kald update metoden
            var result = await _controller.UpdateCustomer(customerId, updatedCustomer);

            // Assert: Verificer at vi får 200 OK med opdateret data
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedCustomer = Assert.IsType<Customer>(okResult.Value);

            // Tjek at data er opdateret
            Assert.Equal(customerId, returnedCustomer.Id);
            Assert.Equal("Updated Name", returnedCustomer.Name);
            Assert.Equal("updated@email.com", returnedCustomer.Email);

            // Verificer at servicen blev kaldt med korrekte parametre
            await _customerService.Received(1).UpdateCustomer(updatedCustomer);
        }

        [Fact]
        public async Task UpdateCustomer_ReturnsNotFound_WhenCustomerDoesNotExist()
        {
            // Arrange: Setup ikke-eksisterende kunde
            var nonExistentId = Guid.NewGuid();
            var customerData = new Customer
            {
                Id = nonExistentId,
                OrganizationId = Guid.NewGuid(),
                Name = "Does Not Exist",
                Email = "notfound@email.com"
            };

            // Mock servicen til at returnere null (kunde ikke fundet)
            _customerService.UpdateCustomer(customerData).Returns((Customer?)null);

            // Act: Prøv at opdatere ikke-eksisterende kunde
            var result = await _controller.UpdateCustomer(nonExistentId, customerData);

            // Assert: Verificer at vi får 404 Not Found
            Assert.IsType<NotFoundResult>(result);

            // Verificer at servicen blev kaldt
            await _customerService.Received(1).UpdateCustomer(customerData);
        }
        #endregion

        #region DeleteCustomer Tests
        [Fact]
        public async Task DeleteCustomer_ReturnsNoContent_WhenDeletionSucceeds()
        {
            // Arrange: ID for kunden der skal slettes
            var customerId = Guid.NewGuid();
            // Simuler at sletning lykkes
            _customerService.DeleteCustomer(customerId).Returns(true);
            // Act: Slet kunden
            var result = await _controller.DeleteCustomer(customerId);
            // Assert: Tjek at vi fik 204 No Content
            Assert.IsType<NoContentResult>(result);
            // Verificer at servicen blev kaldt med det rigtige ID
            await _customerService.Received(1).DeleteCustomer(customerId);
        }

        [Fact]
        public async Task DeleteCustomer_ReturnsNotFound_WhenCustomerDoesNotExist()
        {
            // Arrange: ID for en ikke eksisterende kunde
            var nonExistentId = Guid.NewGuid();
            // Simuler at sletning fejler (kunde findes ikke)
            _customerService.DeleteCustomer(nonExistentId).Returns(false);
            // Act: Prøv at slet kunden
            var result = await _controller.DeleteCustomer(nonExistentId);
            // Assert: Tjek at vi fik 404 Not Found
            Assert.IsType<NotFoundResult>(result);
            // Verificer at servicen blev kaldt med det rigtige ID
            await _customerService.Received(1).DeleteCustomer(nonExistentId);
        }
        #endregion
    }
}
