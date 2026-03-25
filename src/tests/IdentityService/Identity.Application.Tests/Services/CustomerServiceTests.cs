using CCP.Shared.AuthContext;
using CCP.Shared.ResultAbstraction;
using IdentityService.Application.Services.Customer;
using IdentityService.Application.Services.Group;
using IdentityService.Application.Services.Organization;
using IdentityService.Application.Services.User;
using Keycloak.Sdk.services.management;
using Keycloak.Sdk.services.members;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Identity.Application.Tests.Services
{
    public class CustomerServiceTests
    {
        private readonly ILogger<CustomerService> _logger;
        private readonly IOrganizationService _organizationService;
        private readonly ICurrentUser _currentUser;
        private readonly IGroupService _groupService;
        private readonly IUserService _userService;
        private readonly IManagementKeycloakService _managementService;
        private readonly IMemberKeycloakService _memberService;
        private readonly ICustomerService _customerService;

        public CustomerServiceTests()
        {
            _logger = Substitute.For<ILogger<CustomerService>>();
            _organizationService = Substitute.For<IOrganizationService>();
            _currentUser = Substitute.For<ICurrentUser>();
            _groupService = Substitute.For<IGroupService>();
            _userService = Substitute.For<IUserService>();
            _managementService = Substitute.For<IManagementKeycloakService>();
            _memberService = Substitute.For<IMemberKeycloakService>();
            _customerService = new CustomerService(_logger, _organizationService, _currentUser, _groupService, _userService, _managementService, _memberService);
        }

        [Fact]
        public async Task InviteCustomer_ShouldCreateUserAndInviteToOrganization()
        {
            // Arrange
            string Email = "test@example.com";
            CancellationToken CT = TestContext.Current.CancellationToken;
            Guid UserID = Guid.NewGuid();
            Guid OrgId = Guid.NewGuid();

            _currentUser.OrganizationId.Returns(OrgId);

            _userService.CreateCustomer(Email, CT)
                        .Returns(Result.Success(UserID));

            _organizationService.InviteExistingUserToOrganization(OrgId, UserID, CT)
                                        .Returns(Result.Success());

            _groupService.AddUserToGroup("Customers", OrgId, UserID, CT).Returns(Result.Success());

            _managementService.ExecuteEmailRequiredActions(Email, UserID.ToString(), Arg.Any<List<string>>(), lifespan: (24 * 60 * 60), ct: CT).Returns(Result.Success());

            // Act

            var result = await _customerService.InviteCustomer(Email, CT);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
        }
    }
}
