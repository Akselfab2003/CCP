using CCP.Shared.AuthContext;
using CCP.Shared.ResultAbstraction;
using IdentityService.Application.Models;
using IdentityService.Application.Services.Member;
using Keycloak.Sdk.Models;
using Keycloak.Sdk.services.members;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Identity.Application.Tests.Services
{
    public class MemberServiceTests
    {
        private readonly ILogger<MemberService> _logger;
        private readonly IMemberKeycloakService _memberKeycloakService;
        private readonly ICurrentUser _currentUser;
        private readonly MemberService _memberService;

        public MemberServiceTests()
        {
            _logger = NSubstitute.Substitute.For<ILogger<MemberService>>();
            _memberKeycloakService = NSubstitute.Substitute.For<IMemberKeycloakService>();
            _currentUser = NSubstitute.Substitute.For<ICurrentUser>();
            _memberService = new MemberService(_logger, _memberKeycloakService, _currentUser);
        }

        [Fact]
        public async Task GetAllTenantMembers_ReturnsSuccess_WhenMembersRetrieved()
        {
            // Arrange
            Guid orgId = Guid.NewGuid();
            _currentUser.OrganizationId.Returns(orgId);
            List<KeycloakTenantMember> members = new List<KeycloakTenantMember>()
            {
                new KeycloakTenantMember()
                {
                    Id = Guid.NewGuid(),
                    Email = "Admin@test.dk",
                    FirstName = "Test",
                    LastName = "Test",
                    Roles = ["org.Admin"],
                    Groups = ["Admins"],
                },
                new KeycloakTenantMember()
                {

                    Id = Guid.NewGuid(),
                    Email = "Customer@test.dk",
                    FirstName = "cus",
                    LastName = "Test",
                    Roles = ["org.Customer"],
                    Groups = ["Customers"],
                }
            };

            _memberKeycloakService.GetAllMembersOfOrganization(orgId, CancellationToken.None).Returns(Result.Success(members));
            // Act
            Result<List<TenantMemberDto>> result = await _memberService.GetAllTenantMembers();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value);
        }

        [Fact]
        public async Task GetAllTenantMembers_ReturnsFailure_WhenNoOrgId()
        {
            // Arrange
            _currentUser.OrganizationId.Returns(Guid.Empty);
            // Act
            Result<List<TenantMemberDto>> result = await _memberService.GetAllTenantMembers();
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("GetTenantMembersNoOrgId", result.Error.Code);
        }

        [Fact]
        public async Task GetAllTenantMembers_ReturnsFailure_WhenKeycloakServiceFails()
        {
            // Arrange
            Guid orgId = Guid.NewGuid();
            _currentUser.OrganizationId.Returns(orgId);
            _memberKeycloakService.GetAllMembersOfOrganization(orgId, CancellationToken.None)
                                  .Returns(Result.Failure<List<KeycloakTenantMember>>(Error.Failure("KeycloakError", "Failed to retrieve members from Keycloak")));

            // Act
            Result<List<TenantMemberDto>> result = await _memberService.GetAllTenantMembers();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("GetTenantMembersFailed", result.Error.Code);
        }

        [Fact]
        public async Task GetAllTenantMembers_ReturnsFailure_WhenExceptionThrown()
        {
            // Arrange
            Guid orgId = Guid.NewGuid();
            _currentUser.OrganizationId.Returns(orgId);
            _memberKeycloakService.GetAllMembersOfOrganization(orgId, CancellationToken.None)
                .ThrowsAsync(new Exception("Unexpected error"));
            // Act
            Result<List<TenantMemberDto>> result = await _memberService.GetAllTenantMembers();
            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("GetTenantMembersFailed", result.Error.Code);
        }
    }
}
