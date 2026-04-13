using CCP.Website.Services;
using IdentityService.Sdk.Services.Customer;
using IdentityService.Sdk.Services.Supporter;
using IdentityService.Sdk.Services.Tenant;
using IdentityService.Sdk.Services.User;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
namespace CPP.UI.Tests.Fixtures.Website
{
    public class TestFactory : WebApplicationFactory<CCP.Website.Program>
    {
        public IWebsiteReferencesService websiteReferencesServiceMock { get; private set; } = default!;
        public IUserService userServiceMock { get; private set; } = default!;
        public ITenantService tenantServiceMock { get; private set; } = default!;
        public ICustomerService customerServiceMock { get; private set; } = default!;
        public ISupporterService supporterServiceMock { get; private set; } = default!;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test");

            builder.ConfigureServices(services =>
            {

                services.RemoveService<IWebsiteReferencesService>();
                services.RemoveService<IUserService>();
                services.RemoveService<ITenantService>();
                services.RemoveService<ICustomerService>();
                services.RemoveService<ISupporterService>();

                services.AddMockedScoped<IWebsiteReferencesService>();
                services.AddMockedScoped<IUserService>();
                services.AddMockedScoped<ITenantService>();
                services.AddMockedScoped<ICustomerService>();
                services.AddMockedScoped<ISupporterService>();

                services.AddScoped<AuthenticationStateProvider, FakeAuthStateProvider>();

            });

        }

    }
}
