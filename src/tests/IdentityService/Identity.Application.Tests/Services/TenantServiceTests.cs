using CCP.Shared.ResultAbstraction;
using IdentityService.Application.Models;
using IdentityService.Application.Services.Group;
using IdentityService.Application.Services.Organization;
using IdentityService.Application.Services.Tenant;
using IdentityService.Application.Services.User;
using Microsoft.Extensions.Logging;
using NSubstitute;
namespace Identity.Application.Tests.Services
{
    public class TenantServiceTests
    {
        private readonly ILogger<TenantService> _logger;
        private readonly IUserService _userService;
        private readonly IOrganizationService _organizationService;
        private readonly IGroupService _groupService;
        private readonly TenantService _tenantService;

        public TenantServiceTests()
        {
            _logger = Substitute.For<ILogger<TenantService>>();
            _userService = Substitute.For<IUserService>();
            _organizationService = Substitute.For<IOrganizationService>();
            _groupService = Substitute.For<IGroupService>();
            _tenantService = new TenantService(_logger, _userService, _organizationService, _groupService);
        }


        private CreateTenantRequest GetValidCreateTenantRequest()
        {
            return new CreateTenantRequest()
            {
                DomainName = "testdomain.com",
                OrganizationName = "Test Organization",
                AdminUser = new CreateAdminUserRequest()
                {
                    FirstName = "Admin",
                    LastName = "User",
                    Email = "test@testdomain.com",
                    Password = Guid.NewGuid().ToString(),
                }
            };
        }


        [Fact]
        public async Task CreateTenant_ShouldReturnSuccess_WhenAllOperationsSucceed()
        {
            // Arrange
            var req = GetValidCreateTenantRequest();

            _organizationService.CreateOrganization(req.OrganizationName,
                                                    req.DomainName,
                                                    CancellationToken.None)
                                .Returns(Guid.NewGuid());
            _groupService.CreateDefaultGroupsForOrganization(Arg.Any<Guid>(),
                                                             CancellationToken.None)
                         .Returns(Result.Success());

            _userService.CreateUser(req.AdminUser.Email,
                                    req.AdminUser.FirstName,
                                    req.AdminUser.LastName,
                                    req.AdminUser.Password,
                                    CancellationToken.None)
                        .Returns(Result.Success(Guid.NewGuid()));

            _organizationService.InviteExistingUserToOrganization(Arg.Any<Guid>(),
                                                                  Arg.Any<Guid>(),
                                                                  CancellationToken.None)
                                .Returns(Result.Success());

            _groupService.AddUserToGroup(GroupNames.Admins.ToString(),
                                         Arg.Any<Guid>(),
                                         Arg.Any<Guid>(),
                                         CancellationToken.None)
                         .Returns(Result.Success());

            // Act

            var result = await _tenantService.CreateTenant(req);

            // Assert
            Assert.True(result.IsSuccess);
        }


        [Fact]
        public async Task CreateTenant_ShouldReturnFailure_WhenOrganizationCreationFails()
        {
            // Arrange
            var req = GetValidCreateTenantRequest();


            _organizationService.CreateOrganization(req.OrganizationName,
                                                    req.DomainName,
                                                    CancellationToken.None)
                                .Returns(Result.Failure<Guid>(Error.Failure("error", "error")));

            // Act
            var result = await _tenantService.CreateTenant(req);

            //  Assert
            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);

            Assert.Equal("OrganizationCreationFailed", result.Error.Code);
            Assert.Equal("Failed to create organization for the tenant.", result.Error.Description);
        }


        [Fact]
        public async Task CreateTenant_ShouldReturnFailure_WhenGroupCreationFails()
        {
            // Arrange
            var req = GetValidCreateTenantRequest();


            _organizationService.CreateOrganization(req.OrganizationName,
                                                    req.DomainName,
                                                    CancellationToken.None)
                                .Returns(Guid.NewGuid());

            _groupService.CreateDefaultGroupsForOrganization(Arg.Any<Guid>(),
                                                             CancellationToken.None).Returns(Result.Failure(Error.NullValue));

            // Act
            var result = await _tenantService.CreateTenant(req);

            // Assert
            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);

            Assert.Equal("GroupCreationFailed", result.Error.Code);
            Assert.Equal("Failed to create default groups for the tenant.", result.Error.Description);
        }

        [Fact]
        public async Task CreateTenant_ShouldReturnFailure_WhenAdminUserCreationFails()
        {
            // Arrange
            var req = GetValidCreateTenantRequest();

            _organizationService.CreateOrganization(req.OrganizationName,
                                                    req.DomainName,
                                                    CancellationToken.None)
                                .Returns(Guid.NewGuid());

            _groupService.CreateDefaultGroupsForOrganization(Arg.Any<Guid>(),
                                                             CancellationToken.None).Returns(Result.Success());


            _userService.CreateUser(req.AdminUser.Email,
                                    req.AdminUser.FirstName,
                                    req.AdminUser.LastName,
                                    req.AdminUser.Password,
                                    CancellationToken.None)
                        .Returns(Result.Failure<Guid>(Error.NullValue));

            // Act
            var result = await _tenantService.CreateTenant(req);

            // Assert
            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal("AdminUserCreationFailed", result.Error.Code);
            Assert.Equal("Failed to create admin user for the tenant.", result.Error.Description);
        }

        [Fact]
        public async Task CreateTenant_ShouldReturnFailure_WhenAddingAdminToOrganizationFails()
        {
            // Arrange
            var req = GetValidCreateTenantRequest();
            _organizationService.CreateOrganization(req.OrganizationName,
                                                    req.DomainName,
                                                    CancellationToken.None)
                                .Returns(Guid.NewGuid());
            _groupService.CreateDefaultGroupsForOrganization(Arg.Any<Guid>(),
                                                             CancellationToken.None).Returns(Result.Success());

            _userService.CreateUser(req.AdminUser.Email,
                                    req.AdminUser.FirstName,
                                    req.AdminUser.LastName,
                                    req.AdminUser.Password,
                                    CancellationToken.None)
                        .Returns(Result.Success(Guid.NewGuid()));

            _organizationService.InviteExistingUserToOrganization(Arg.Any<Guid>(),
                                                                  Arg.Any<Guid>(),
                                                                  CancellationToken.None)
                                .Returns(Result.Failure(Error.NullValue));

            // Act
            var result = await _tenantService.CreateTenant(req);

            // Assert
            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal("AddAdminToOrganizationFailed", result.Error.Code);
            Assert.Equal("Failed to add admin user to the tenant organization.", result.Error.Description);
        }

        [Fact]
        public async Task CreateTenant_ShouldReturnFailure_WhenAddingAdminToGroupFails()
        {
            // Arrange
            var req = GetValidCreateTenantRequest();
            _organizationService.CreateOrganization(req.OrganizationName,
                                                    req.DomainName,
                                                    CancellationToken.None)
                                .Returns(Guid.NewGuid());
            _groupService.CreateDefaultGroupsForOrganization(Arg.Any<Guid>(),
                                                             CancellationToken.None).Returns(Result.Success());
            _userService.CreateUser(req.AdminUser.Email,
                                    req.AdminUser.FirstName,
                                    req.AdminUser.LastName,
                                    req.AdminUser.Password,
                                    CancellationToken.None)
                        .Returns(Result.Success(Guid.NewGuid()));
            _organizationService.InviteExistingUserToOrganization(Arg.Any<Guid>(),
                                                                  Arg.Any<Guid>(),
                                                                  CancellationToken.None)
                                .Returns(Result.Success());
            _groupService.AddUserToGroup(GroupNames.Admins.ToString(),
                                         Arg.Any<Guid>(),
                                         Arg.Any<Guid>(),
                                         CancellationToken.None)
                         .Returns(Result.Failure(Error.NullValue));
            // Act
            var result = await _tenantService.CreateTenant(req);
            // Assert
            Assert.True(result.IsFailure);
            Assert.False(result.IsSuccess);
            Assert.Equal("AddAdminToGroupFailed", result.Error.Code);
            Assert.Equal("Failed to add admin user to the Admins group.", result.Error.Description);
        }
    }
}
