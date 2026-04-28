using ChatApp.Encryption;
using CustomerService.Api.DB;
using CustomerService.Sdk.ServiceDefaults;
using TestUtils.Integration;

namespace Customer.Api.Integration.Tests.Fixtures
{
    public class CustomerServiceFixture : GenericIntegrationSdkAndDbTestFixture<CustomerDBContext>, IAsyncLifetime
    {
        public override string DBResourceName => "customerdb";

        public override List<string> RequiredResources => [
                "keycloak",
                "customerdb",
                "customerservice-api",
                "postgres"];

        public override string APIResourceName => "customerservice-api";

        public async ValueTask InitializeAsync()
        {
            await Initialize();
            var encryptionKey = GetConfiguration()["Encryption_Key"]
                ?? throw new InvalidOperationException("Encryption_Key configuration value is required.");
            DB_Services.AddSingleton<IEncryptionService>(new AesEncryptionService(encryptionKey));
            SDK_Services.AddHttpContextAccessor();
            SDK_Services.AddCustomerviceSdk(GetServiceUrl(APIResourceName), true);
            await BuildProviders();
        }

        public async ValueTask DisposeAsync()
        {
            await Dispose();
        }
    }
}
