using System.Security.Cryptography;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Duende.AccessTokenManagement;
using Duende.IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace TestUtils.Integration
{
    public abstract class GenericIntegrationTestFixture
    {
        public abstract List<string> RequiredResources { get; }
        public abstract string APIResourceName { get; }
        public IServiceProvider SDK => SDK_Provider;
        private IServiceProvider SDK_Provider = null!;
        public IServiceCollection SDK_Services = new ServiceCollection();


        internal DistributedApplication App = null!;
        private IDistributedApplicationTestingBuilder AppHost = null!;
        public TimeSpan DefaultTimeout = TimeSpan.FromMinutes(1);
        public bool IsRemoveNotNeededResourcesForTestingEnabled = true;
        public IConfiguration GetConfiguration() => App.Services.GetRequiredService<IConfiguration>();

        public string GetServiceUrl(string ResourceName, string endpointName = "") => App.GetEndpointForNetwork(ResourceName, null, endpointName: endpointName).AbsoluteUri;

        public virtual async Task Initialize()
        {
            await InitializeApp();
            await InitializeSDK();
        }

        public virtual async Task BuildProviders()
        {
            SDK_Provider = SDK_Services.BuildServiceProvider();
        }
        private string GenerateEncryptionKey()
        {
            var keyBase = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

            var decoded = Convert.FromBase64String(keyBase);

            if (decoded.Length != 32)
            {
                throw new InvalidOperationException("Generated encryption key is not 256 bits (32 bytes) long.");
            }

            return keyBase;
        }
        private async Task InitializeApp()
        {
            var ct = TestContext.Current.CancellationToken;

            var encryption_key = GenerateEncryptionKey();

            AppHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.CCP_AppHost>(
            [
                "DcpPublisher:RandomizePorts=false",
                $"KeycloakAdminApiClientSecret={Guid.NewGuid()}",
                "ENVIORMENT=Tests",
                $"SERVICE_ACCOUNT_SECRET={Guid.NewGuid()}",
                "ROUNDCUBE_DEFAULT_USER_EMAIL=test@test.test",
                "ROUNDCUBE_DEFAULT_USER_PASSWORD=test",
                "emailWorkerServiceUsername=test@test.test",
                "emailWorkerServicePassword=test",
                $"Encryption_Key={encryption_key}",
                "emailHostUrl=localhost",
                $"MAILCOW_API_KEY={Guid.NewGuid()}",
                $"MAILCOW_API_URL=http://localhost:8080/api/v1"
            ], configureBuilder: (config, host) =>
            {
                config.TrustDeveloperCertificate = true;
            }, ct);

            if (IsRemoveNotNeededResourcesForTestingEnabled)
            {
                RemoveNotNeededResourcesForTesting();
            }

            AppHost.Services.AddLogging(builder =>
            {
                builder.AddConsole();
            });


            try
            {
                App = await AppHost.BuildAsync(ct).WaitAsync(DefaultTimeout, ct);
                await App.StartAsync(ct).WaitAsync(DefaultTimeout, ct);
                await App.ResourceNotifications.WaitForResourceHealthyAsync(resourceName: APIResourceName, ct).WaitAsync(DefaultTimeout, ct);
            }
            catch (Exception)
            {
                foreach (var res in AppHost.Resources)
                {
                    var healthStatuses = App.ResourceNotifications.TryGetCurrentState(res.Name, out var resourceEvent);
                    if (healthStatuses && resourceEvent != null)
                    {
                        var healthStatus = resourceEvent.Snapshot.HealthReports;
                        foreach (var status in healthStatus)
                        {
                            if (status.Status.HasValue)
                            {
                                if (status.Status.Value != Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy)
                                {
                                    throw new Exception($"Resource: {res.Name}, is unhealthy. Status: {status.Status.Value}, Description: {status.Description}");
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Resource: {res.Name}, Health status not available.");
                    }
                }
            }
        }
        private void RemoveNotNeededResourcesForTesting()
        {
            var resources = AppHost.Resources
                .Where(r => !RequiredResources.Contains(r.Name))
                .ToList();

            foreach (var resource in resources)
            {
                AppHost.Resources.Remove(resource);
            }
        }

        private async Task InitializeSDK()
        {
            IConfiguration configuration = App.Services.GetRequiredService<IConfiguration>();

            SDK_Services.AddLogging();

            // Configure OAuth2 client credentials for authentication
            SDK_Services.AddClientCredentialsTokenManagement()
                        .AddClient(ClientCredentialsClientName.Parse("CCP.ServiceAccount"), client =>
                        {
                            client.TokenEndpoint = new Uri("http://localhost:8080/realms/CCP/protocol/openid-connect/token");
                            client.ClientId = ClientId.Parse("CCP.ServiceAccount");
                            client.ClientSecret = ClientSecret.Parse(
                                configuration["SERVICE_ACCOUNT_SECRET"]
                                ?? throw new InvalidOperationException("SERVICE_ACCOUNT_SECRET configuration value is required.")
                            );
                            client.Scope = Scope.ParseOrDefault("openid");
                            client.ClientCredentialStyle = ClientCredentialStyle.AuthorizationHeader;
                        });
        }

        public async ValueTask Dispose()
        {
            if (App != null)
            {
                await App.DisposeAsync();
            }
        }

    }
}
