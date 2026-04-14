using IdentityService.Application.Services.Customer;
using IdentityService.Application.Services.Group;
using IdentityService.Application.Services.Member;
using IdentityService.Application.Services.Organization;
using IdentityService.Application.Services.Supporter;
using IdentityService.Application.Services.Tenant;
using IdentityService.Application.Services.User;
using IdentityService.Application.Services.UserRights;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityService.Application.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>()
                    .AddScoped<IGroupService, GroupService>()
                    .AddScoped<IOrganizationService, OrganizationService>()
                    .AddScoped<ITenantService, TenantService>()
                    .AddScoped<IMemberService, MemberService>()
                    .AddScoped<ICustomerService, CustomerService>()
                    .AddScoped<ISupporterService, SupporterService>()
                    .AddScoped<IUserRightsManagementService, UserRightsManagementService>();

        }
    }
}
