using CCP.Shared.ResultAbstraction;
using IdentityService.Application.Services.Group;
using Keycloak.Sdk.services.groups;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Identity.Application.Tests.Services
{
    public class GroupServiceTests
    {
        private readonly IGroupService _groupService;
        private readonly ILogger<GroupService> _logger;
        private readonly IGroupKeycloakService _groupKeycloakService;

        public GroupServiceTests()
        {
            _logger = NSubstitute.Substitute.For<ILogger<GroupService>>();
            _groupKeycloakService = NSubstitute.Substitute.For<IGroupKeycloakService>();
            _groupService = new GroupService(_logger, _groupKeycloakService);
        }


        [Fact]
        public async Task CreateDefaultGroupsForOrganization_SuccessfullyCreatesGroups_ReturnsSuccess()
        {
            // Arrange
            var orgID = Guid.NewGuid();
            var parentGroupID = Guid.NewGuid();

            _groupKeycloakService.CreateGroupAsync(Arg.Any<Guid?>(),
                                                   Arg.Any<string>(),
                                                   Arg.Any<List<string>>(),
                                                   Arg.Any<CancellationToken>())
                                  .Returns(Task.FromResult(Result.Success(parentGroupID)),
                                             Task.FromResult(Result.Success(Guid.NewGuid())));

            // Act
            var result = await _groupService.CreateDefaultGroupsForOrganization(orgID, TestContext.Current.CancellationToken);
            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.False(result.IsFailure);

            await _groupKeycloakService.Received().CreateGroupAsync(ParentGroupID: null,
                                                                    groupName: $"org-{orgID}",
                                                                    Roles: null,
                                                                    ct: TestContext.Current.CancellationToken);
            await _groupKeycloakService.Received().CreateGroupAsync(ParentGroupID: parentGroupID,
                                                                    groupName: "Admins",
                                                                    Roles: Arg.Is<List<string>>(roles => roles.SequenceEqual(new[] { "org.Admin" })),
                                                                    ct: TestContext.Current.CancellationToken);
            await _groupKeycloakService.Received().CreateGroupAsync(ParentGroupID: parentGroupID,
                                                                    groupName: "Managers",
                                                                    Roles: Arg.Is<List<string>>(roles => roles.SequenceEqual(new[] { "org.Manager" })),
                                                                    ct: TestContext.Current.CancellationToken);
            await _groupKeycloakService.Received().CreateGroupAsync(ParentGroupID: parentGroupID,
                                                                    groupName: "Supporters",
                                                                    Roles: Arg.Is<List<string>>(roles => roles.SequenceEqual(new[] { "org.Supporter" })),
                                                                    ct: TestContext.Current.CancellationToken);
            await _groupKeycloakService.Received().CreateGroupAsync(ParentGroupID: parentGroupID,
                                                                    groupName: "Customers",
                                                                    Roles: Arg.Is<List<string>>(roles => roles.SequenceEqual(new[] { "org.Customer" })),
                                                                    ct: TestContext.Current.CancellationToken);
        }


        [Fact]
        public async Task CreateDefaultGroupsForOrganization_KeycloakServiceThrowsException_ReturnsFailure()
        {
            // Arrange
            var orgID = Guid.NewGuid();
            var exceptionMessage = "Keycloak service error";
            _groupKeycloakService.CreateGroupAsync(Arg.Any<Guid?>(),
                                                   Arg.Any<string>(),
                                                   Arg.Any<List<string>>(),
                                                   Arg.Any<CancellationToken>())
                                  .Throws(new Exception(exceptionMessage));
            // Act
            var result = await _groupService.CreateDefaultGroupsForOrganization(orgID, TestContext.Current.CancellationToken);
            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Equal(expected: "CreateDefaultGroupsForOrganization.failed", actual: result.Error.Code);
        }


        [Fact]
        public async Task CreateDefaultGroupsForOrganization_ParentGroupCreationFails_ReturnsFailure()
        {
            // Arrange
            var orgID = Guid.NewGuid();
            var exceptionMessage = "Keycloak service error during parent group creation";
            _groupKeycloakService.CreateGroupAsync(ParentGroupID: null,
                                                   groupName: $"org-{orgID}",
                                                   Roles: null,
                                                   ct: TestContext.Current.CancellationToken)
                                  .Throws(new Exception(exceptionMessage));

            // Act

            var result = await _groupService.CreateDefaultGroupsForOrganization(orgID, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Equal(expected: "CreateDefaultGroupsForOrganization.failed", actual: result.Error.Code);
        }

        [Fact]
        public async Task CreateDefaultGroupsForOrganization_ChildGroupCreationFails_ReturnsFailure()
        {
            // Arrange
            var orgID = Guid.NewGuid();
            var parentGroupID = Guid.NewGuid();
            var exceptionMessage = "Keycloak service error during child group creation";
            _groupKeycloakService.CreateGroupAsync(ParentGroupID: null,
                                                   groupName: $"org-{orgID}",
                                                   Roles: null,
                                                   ct: TestContext.Current.CancellationToken)
                                  .Returns(Result.Success(parentGroupID));
            _groupKeycloakService.CreateGroupAsync(ParentGroupID: parentGroupID,
                                                   groupName: "Admins",
                                                   Roles: Arg.Is<List<string>>(roles => roles.SequenceEqual(new[] { "org.Admin" })),
                                                   ct: TestContext.Current.CancellationToken)
                                  .Throws(new Exception(exceptionMessage));
            // Act
            var result = await _groupService.CreateDefaultGroupsForOrganization(orgID, TestContext.Current.CancellationToken);
            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Equal(expected: "CreateDefaultGroupsForOrganization.failed", actual: result.Error.Code);
        }

        [Fact]
        public async Task AddUserToGroup_ValidInput_ReturnsSuccess()
        {
            // Arrange
            var groupName = Guid.NewGuid().ToString();
            var userID = Guid.NewGuid();
            var OrgID = Guid.NewGuid();
            _groupKeycloakService.AddUserToGroup(OrgID, UserID: userID.ToString(), groupName).Returns(Result.Success());

            // Act
            var result = await _groupService.AddUserToGroup(groupName, OrgID, userID, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task AddUserToGroup_KeycloakServiceThrowsException_ReturnsFailure()
        {
            // Arrange
            var groupName = Guid.NewGuid().ToString();
            var userID = Guid.NewGuid();
            var OrgID = Guid.NewGuid();
            var exceptionMessage = "Keycloak service error";
            _groupKeycloakService.AddUserToGroup(OrgID, UserID: userID.ToString(), groupName).Throws(new Exception(exceptionMessage));

            // Act
            var result = await _groupService.AddUserToGroup(groupName, OrgID, userID, TestContext.Current.CancellationToken);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Equal(expected: "AddUserToGroup.failed", actual: result.Error.Code);
        }



        [Fact]
        public async Task RemoveUserFromGroup_ValidInput_ReturnsSuccess()
        {
            // Arrange
            var groupName = Guid.NewGuid().ToString();
            var userID = Guid.NewGuid();
            var OrgID = Guid.NewGuid();
            _groupKeycloakService.RemoveUserFromGroup(TenantID: OrgID, GroupName: groupName, UserID: userID.ToString()).Returns(Result.Success());

            // Act
            var result = await _groupService.RemoveUserFromGroup(groupName, OrgID, userID, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task RemoveUserFromGroup_KeycloakServiceThrowsException_ReturnsFailure()
        {
            // Arrange
            var groupName = Guid.NewGuid().ToString();
            var userID = Guid.NewGuid();
            var OrgID = Guid.NewGuid();
            var exceptionMessage = "Keycloak service error";
            _groupKeycloakService.RemoveUserFromGroup(TenantID: OrgID, GroupName: groupName, UserID: userID.ToString()).Throws(new Exception(exceptionMessage));

            // Act
            var result = await _groupService.RemoveUserFromGroup(groupName, OrgID, userID, TestContext.Current.CancellationToken);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.True(result.IsFailure);
            Assert.Equal(expected: "RemoveUserFromGroup.failed", actual: result.Error.Code);
        }
    }
}
