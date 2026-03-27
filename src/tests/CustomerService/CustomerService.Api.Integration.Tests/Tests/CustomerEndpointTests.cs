using System;
using System.Collections.Generic;
using System.Text;
using Customer.Api.Integration.Tests.Fixtures;
using CustomerService.Sdk.Models;
using CustomerService.Sdk.Services;

namespace Customer.Api.Integration.Tests.Tests
{
    [Collection("CustomerApiCollection")]
    public class CustomerEndpointTests : IClassFixture<CustomerServiceFixture>
    {
        private readonly CustomerServiceFixture _fixture;

        public CustomerEndpointTests(CustomerServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task CreateCustomer_ShouldNotThrow_WhenRequestIsValid()
        {
            //Arrange
            using var SDK = _fixture.SDK.CreateScope();
            var customerService = SDK.ServiceProvider.GetRequiredService<ICustomerSdkService>();

            var createRequest = new CreateCustomerRequest
            {
                Id = Guid.NewGuid(),
                OrganizationId = Guid.NewGuid(),
                Name = "Test Customer",
                Email = "test@customer.dk"
            };

            //Act & Assert (ingen exception = success)
            await customerService.CreateCustomer(createRequest);
        }
    }
}
