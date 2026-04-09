using EmailService.Domain.Interfaces;
using EmailService.Infrastructure.Data;
using EmailService.Infrastructure.EmailInfrastructure;
using EmailService.Sdk.ServiceDefaults;
using TestUtils.Integration;

namespace Email.Api.Integration.Tests.Fixtures
{
    /// <summary>
    /// Test fixture for EmailService integration tests.
    /// Manages the lifecycle of the distributed application and provides SDK and DB access.
    /// </summary>
    public class EmailServiceFixture : GenericIntegrationSdkAndDbTestFixture<DBcontext>, IAsyncLifetime
    {
        public override string DBResourceName => "emaildb";
        public override string APIResourceName => "emailservice";

        public override List<string> RequiredResources => [
            APIResourceName,
            DBResourceName,
            "keycloak",
            "postgres"
        // MailServer removed — tests mock SMTP, no real mail server needed
        ];


        public async ValueTask InitializeAsync()
        {
            await Initialize();
            DB_Services.AddScoped<IEmailSent, EmailSentRepo>();
            DB_Services.AddScoped<IEmailReceived, EmailReceivedRepo>();
            SDK_Services.AddEmailServiceSdk(GetServiceUrl(APIResourceName), true);
            await BuildProviders();
        }

        public async ValueTask DisposeAsync()
        {
            await Dispose();
        }
    }
}
