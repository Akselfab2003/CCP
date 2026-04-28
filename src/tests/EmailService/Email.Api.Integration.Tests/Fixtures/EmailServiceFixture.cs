using CCP.Shared.AuthContext;
using ChatApp.Encryption;
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
        public override string APIResourceName => "emailservice-api";

        public override List<string> RequiredResources => [
            APIResourceName,
            DBResourceName,
            "keycloak",
            "postgres",
            "RabbitMQ",
            "customerservice-api",
            "customerdb"
        ];


        public async ValueTask InitializeAsync()
        {
            DefaultTimeout = TimeSpan.FromMinutes(2);
            await Initialize();
            var encryptionKey = GetConfiguration()["Encryption_Key"]
              ?? throw new InvalidOperationException("Encryption_Key configuration value is required.");
            DB_Services.AddSingleton<IEncryptionService>(new AesEncryptionService(encryptionKey));
            DB_Services.AddScoped<ICurrentUser, CurrentUser>();
            DB_Services.AddScoped<IEmailSent, EmailSentRepo>();
            DB_Services.AddScoped<IEmailReceived, EmailReceivedRepo>();
            SDK_Services.AddHttpContextAccessor();
            SDK_Services.AddEmailServiceSdk(GetServiceUrl(APIResourceName), true);
            await BuildProviders();
        }

        public async ValueTask DisposeAsync()
        {
            await Dispose();
        }
    }
}
