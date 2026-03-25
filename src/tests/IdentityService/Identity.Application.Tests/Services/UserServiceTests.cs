using CCP.Shared.ResultAbstraction;
using IdentityService.Application.Services.User;
using Keycloak.Sdk.Models;
using Keycloak.Sdk.services.users;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Identity.Application.Tests.Services
{
    public class UserServiceTests
    {
        private readonly ILogger<UserService> _logger;
        private readonly IUserKeycloakService _userKeycloakService;
        private readonly IUserService _userService;

        public UserServiceTests()
        {
            _logger = NSubstitute.Substitute.For<ILogger<UserService>>();
            _userKeycloakService = NSubstitute.Substitute.For<IUserKeycloakService>();
            _userService = new UserService(_logger, _userKeycloakService);
        }

        [Fact]
        public async Task GetUserDetails_ReturnsUserDetails_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var expectedUser = new Keycloak.Sdk.Models.UserKeycloakAccount
            {
                Id = userId,
                Name = "Test",
                Email = "test@test.test",
            };
            _userKeycloakService.GetUserDetailsByID(userId, TestContext.Current.CancellationToken).Returns(expectedUser);

            // Act
            Result<UserKeycloakAccount> result = await _userService.GetUserDetails(userId, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedUser.Id, result.Value.Id);
            Assert.Equal(expectedUser.Name, result.Value.Name);
            Assert.Equal(expectedUser.Email, result.Value.Email);
        }

        [Fact]
        public async Task GetUserDetails_ReturnsFailure_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userKeycloakService.GetUserDetailsByID(userId, TestContext.Current.CancellationToken).Returns(Result.Failure<UserKeycloakAccount>(Error.Failure("GetUserDetailsByID.failed", "User not found")));

            // Act
            Result<UserKeycloakAccount> result = await _userService.GetUserDetails(userId, TestContext.Current.CancellationToken);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("GetUserDetailsByID.failed", result.Error.Code);
            Assert.Equal("User not found", result.Error.Description);
        }


        [Fact]
        public async Task GetUserDetails_ReturnsFailure_WhenExceptionIsThrown()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userKeycloakService.GetUserDetailsByID(userId, TestContext.Current.CancellationToken).Throws(new Exception("keycloak failed"));
            // Act
            Result<UserKeycloakAccount> result = await _userService.GetUserDetails(userId, TestContext.Current.CancellationToken);
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("FailedToGetUserDetails", result.Error.Code);
            Assert.Equal($"An error occurred while getting user details for user with ID {userId}", result.Error.Description);
        }

        [Fact]
        public async Task SearchUsers_ReturnsUserList_WhenUsersExist()
        {
            // Arrange
            var users = new List<UserKeycloakAccount>
            {
                new UserKeycloakAccount { Id = Guid.NewGuid(), Name = "Test User 1", Email = "test@test.test" },
                new UserKeycloakAccount { Id = Guid.NewGuid(), Name = "Test user 2", Email = "user@user.user" }
            };

            _userKeycloakService.SearchUsers("test", TestContext.Current.CancellationToken).Returns(users);

            // Act
            Result<List<UserKeycloakAccount>> result = await _userService.SearchUsers("test", TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count);
        }

        [Fact]
        public async Task SearchUsers_ReturnsFailure_WhenExceptionIsThrown()
        {
            // Arrange
            _userKeycloakService.SearchUsers("test", TestContext.Current.CancellationToken).Throws(new Exception("keycloak failed"));

            // Act
            Result<List<UserKeycloakAccount>> result = await _userService.SearchUsers("test", TestContext.Current.CancellationToken);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("FailedToSearchUsers", result.Error.Code);
            Assert.Equal($"An error occurred while searching for users with searchTerm 'test'", result.Error.Description);
        }

        [Fact]
        public async Task SearchUsers_ReturnsEmptyList_WhenNoUsersFound()
        {
            // Arrange
            _userKeycloakService.SearchUsers("nonexistent", TestContext.Current.CancellationToken).Returns(new List<UserKeycloakAccount>());

            // Act
            Result<List<UserKeycloakAccount>> result = await _userService.SearchUsers("nonexistent", TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value);
        }
    }
}
