using CCP.Shared.AuthContext;
using CCP.Shared.ResultAbstraction;
using IdentityService.Application.Services.Organization;
using Keycloak.Sdk.services.organizations;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
namespace Identity.Application.Tests.Services
{
    public class OrganizationServiceTests
    {
        private readonly ILogger<OrganizationService> _logger;
        private readonly IOrganizationKeycloakService _organizationKeycloakService;
        private readonly ICurrentUser _currentUser;
        private readonly OrganizationService _organizationService;

        public OrganizationServiceTests()
        {
            _logger = Substitute.For<ILogger<OrganizationService>>();
            _organizationKeycloakService = Substitute.For<IOrganizationKeycloakService>();
            _currentUser = Substitute.For<ICurrentUser>();
            _organizationService = new OrganizationService(_logger, _organizationKeycloakService, _currentUser);
        }


        [Fact]
        public async Task CreateOrganization_ShouldReturnSuccess_WhenOrganizationIsCreated()
        {
            // Arrange
            string organizationName = "Test Organization";
            string domainName = "test.com";
            Guid expectedOrgId = Guid.NewGuid();
            _organizationKeycloakService.CreateOrganizationAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Result.Success(expectedOrgId));

            // Act
            var result = await _organizationService.CreateOrganization(organizationName, domainName, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedOrgId, result.Value);
        }

        [Fact]
        public async Task CreateOrganization_ShouldReturnFailure_WhenOrganizationCreationFails()
        {
            // Arrange
            string organizationName = "Test Organization";
            string domainName = "test.com";

            var expectedError = Error.Failure("CreateOrganization.Error", "Failed to create organization");

            _organizationKeycloakService.CreateOrganizationAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Result.Failure<Guid>(expectedError));

            // Act
            var result = await _organizationService.CreateOrganization(organizationName, domainName, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(expectedError, result.Error);

        }

        [Theory]
        [InlineData("Test Organization", "test_organization")]
        [InlineData("Test-Organization", "test_organization")]
        [InlineData("Test.Organization", "test_organization")]
        [InlineData("Test,Organization", "test_organization")]
        [InlineData("Test'Organization", "test_organization")]
        [InlineData("Test\"Organization", "test_organization")]
        public async Task CreateOrganization_ShouldCleanOrganizationName_BeforeCreatingOrganization(string organizationName, string ExpectedName)
        {
            // Arrange
            string domainName = "test.com";
            Guid expectedOrgId = Guid.NewGuid();
            _organizationKeycloakService.CreateOrganizationAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Result.Success(expectedOrgId));

            // Act
            var result = await _organizationService.CreateOrganization(organizationName, domainName, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedOrgId, result.Value);
            await _organizationKeycloakService.Received(1).CreateOrganizationAsync(ExpectedName, domainName, Arg.Any<CancellationToken>());
        }


        [Fact]
        public async Task InviteExistingUserToOrganization_ShouldReturnSuccess_WhenUserIsInvited()
        {
            // Arrange
            Guid orgId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            _organizationKeycloakService.AddExistingMemberToOrganizationAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(Result.Success());

            // Act
            var result = await _organizationService.InviteExistingUserToOrganization(orgId, userId, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task InviteExistingUserToOrganization_ShouldReturnFailure_WhenInvitationFails()
        {
            // Arrange
            Guid orgId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            var expectedError = Error.Failure("InviteExistingUserToOrganization.Error", "Failed to invite user to organization");
            _organizationKeycloakService.AddExistingMemberToOrganizationAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(Result.Failure(expectedError));

            // Act
            var result = await _organizationService.InviteExistingUserToOrganization(orgId, userId, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal(expectedError, result.Error);
        }

        [Fact]
        public async Task InviteExistingUserToOrganization_ShouldLogError_WhenExceptionIsThrown()
        {
            // Arrange
            Guid orgId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            var exception = new Exception("Test exception");
            _organizationKeycloakService.AddExistingMemberToOrganizationAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Throws(exception);

            // Act
            var result = await _organizationService.InviteExistingUserToOrganization(orgId, userId, TestContext.Current.CancellationToken);

            // Assert
            _logger.Received().Log(LogLevel.Error,
                                   Arg.Any<EventId>(),
                                   Arg.Is<object>(o => o != null && o.ToString()!.Contains("Error inviting existing user with ID")),
                                   Arg.Any<Exception>(),
                                   Arg.Any<Func<object, Exception?, string>>());
        }

        [Fact]
        public async Task InviteNewUserToJoinOrganization_ShouldReturnSuccess_WhenUserIsInvited()
        {
            // Arrange
            Guid orgId = Guid.NewGuid();
            string email = "test@test.dk";

            _currentUser.OrganizationId.Returns(orgId);
            _organizationKeycloakService.InviteMemberToOrg(orgId, email, Arg.Any<CancellationToken>())
                .Returns(Result.Success());

            // Act
            var result = await _organizationService.InviteNewUserToJoinOrganization(email, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);
        }

        [Fact]
        public async Task InviteNewUserToJoinOrganization_ShouldReturnFailure_WhenInvitationFails()
        {
            // Arrange
            Guid orgId = Guid.NewGuid();

            _currentUser.OrganizationId.Returns(orgId);

            _organizationKeycloakService.InviteMemberToOrg(orgId, Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Result.Failure(Error.Failure("InviteNewUserToJoinOrganization.Error", "Failed to invite new user to organization")));

            // Act
            var result = await _organizationService.InviteNewUserToJoinOrganization("", TestContext.Current.CancellationToken);

            // Assert

            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal("InviteNewUserToJoinOrganization.Error", result.Error.Code);
        }

        [Fact]
        public async Task InviteNewUserToJoinOrganization_ShouldReturnError_WhenExceptionIsThrown()
        {
            // Arrange
            Guid orgId = Guid.NewGuid();
            var exception = new Exception("Test exception");
            _currentUser.OrganizationId.Returns(orgId);
            _organizationKeycloakService.InviteMemberToOrg(orgId, Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Throws(exception);

            // Act
            var result = await _organizationService.InviteNewUserToJoinOrganization("", TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal("InviteNewUser.Error", result.Error.Code);
            _logger.Received().Log(LogLevel.Error,
                                Arg.Any<EventId>(),
                                Arg.Is<object>(o => o != null && o.ToString()!.Contains("Error inviting new user with email")),
                                Arg.Any<Exception>(),
                                Arg.Any<Func<object, Exception?, string>>());
        }
    }
}
