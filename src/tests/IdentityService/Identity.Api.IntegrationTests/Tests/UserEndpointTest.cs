using CCP.Shared.ResultAbstraction;
using Identity.Api.IntegrationTests.Fixtures;
using IdentityService.Sdk.Services.Tenant;

namespace Identity.Api.IntegrationTests.Tests
{
    [Collection("Identity")]
    public class UserEndpointTest
    {
        private readonly IdentityServiceFixture _fixture;
        public UserEndpointTest(IdentityServiceFixture fixture)
        {
            _fixture = fixture;
        }

        private string ErrorMessage(Result res) => $"Error Code: {res.Error.Code}, Description: {res.Error.Description}, Type: {res.Error.Type}";

        private void ValidateCreateTenantResult()
        {
            using var Keycloak_SDK = _fixture.Keycloak_SDK.CreateScope();

        }


        [Fact]
        public async Task CreateTenant_ShouldSuccess_WhenRequestIsValid()
        {
            // Arrange
            using var Keycloak_SDK = _fixture.Keycloak_SDK.CreateScope();
            using var SDK = _fixture.SDK.CreateScope();

            ITenantService TenantService = SDK.ServiceProvider.GetRequiredService<ITenantService>();

            var reqBody = new IdentityService.Sdk.Models.CreateTenantDTO()
            {
                OrganizationName = "TestOrganization",
                DomainName = "testorganization.com",
                AdminUser = new IdentityService.Sdk.Models.CreateAdminUserDTO()
                {
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "test@test.dk",
                    Password = "Password123!"
                }
            };

            // Act
            Result CreateTenantResult = await TenantService.CreateTenant(reqBody, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(CreateTenantResult.IsSuccess, ErrorMessage(CreateTenantResult));


        }
    }
}
