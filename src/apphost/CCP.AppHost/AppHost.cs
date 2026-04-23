using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

// Load environment-specific configuration
string Environment = builder.Configuration.GetValue("ENVIORMENT", "DEV");
string KeycloakApiClientSecret = builder.Configuration.GetValue<string>("KeycloakAdminApiClientSecret")
    ?? throw new InvalidOperationException("KeycloakAdminApiClientSecret configuration value is required.");
string RealmImportPath = builder.Configuration.GetValue<string>("REALM_IMPORT_PATH")
    ?? throw new InvalidOperationException("REALM_IMPORT_PATH configuration value is required.");
string ServiceAccountSecret = builder.Configuration.GetValue<string>("SERVICE_ACCOUNT_SECRET")
    ?? throw new InvalidOperationException("SERVICE_ACCOUNT_SECRET configuration value is required.");
string RoundCubeDefaultUserEmail = builder.Configuration.GetValue<string>("ROUNDCUBE_DEFAULT_USER_EMAIL")
    ?? throw new InvalidOperationException("ROUNDCUBE_DEFAULT_USER_EMAIL configuration value is required when mail infrastructure is enabled.");
string RoundCubeDefaultUserPassword = builder.Configuration.GetValue<string>("ROUNDCUBE_DEFAULT_USER_PASSWORD")
    ?? throw new InvalidOperationException("ROUNDCUBE_DEFAULT_USER_PASSWORD configuration value is required when mail infrastructure is enabled.");
string EncryptionKey = builder.Configuration.GetValue<string>("Encryption_Key")
    ?? throw new InvalidOperationException("Encryption_Key configuration value is required.");
string EmailWorkerServiceUsername = builder.Configuration.GetValue<string>("emailWorkerServiceUsername")
    ?? throw new InvalidOperationException("emailWorkerServiceUsername configuration value is required.");
string EmailWorkerServicePassword = builder.Configuration.GetValue<string>("emailWorkerServicePassword")
    ?? throw new InvalidOperationException("emailWorkerServicePassword configuration value is required.");
string EmailHostUrl = builder.Configuration.GetValue<string>("emailHostUrl")
    ?? throw new InvalidOperationException("emailHostUrl configuration value is required.");

string MAILCOW_API_KEY = builder.Configuration.GetValue<string>("MAILCOW_API_KEY")
    ?? throw new InvalidOperationException("MAILCOW_API_KEY configuration value is required when mail infrastructure is enabled.");
string MAILCOW_API_URL = builder.Configuration.GetValue<string>("MAILCOW_API_URL")
    ?? throw new InvalidOperationException("MAILCOW_API_URL configuration value is required when mail infrastructure is enabled.");
ContainerLifetime LifeTimeMode = Environment == "DEV" ? ContainerLifetime.Persistent : ContainerLifetime.Session;


// External Services
IResourceBuilder<OllamaResource> Ollama = builder.AddOllama("ollama");
IResourceBuilder<KeycloakResource> Keycloak = builder.AddKeycloak("keycloak", 8080);
IResourceBuilder<PostgresServerResource> Postgres = builder.AddPostgres("postgres");
IResourceBuilder<ContainerResource> Roundcube = builder.AddContainer("Roundcube", "roundcube/roundcubemail:latest");
IResourceBuilder<RabbitMQServerResource> RabbitMq = builder.AddRabbitMQ("RabbitMQ");

// Configure External Services
Postgres.WithImage("pgvector/pgvector", "pg16")
        .WithBindMount("./init-db", "/docker-entrypoint-initdb.d")
        .WithLifetime(LifeTimeMode)
        .WithOtlpExporter();

Ollama.WithOtlpExporter()
      .WithLifetime(LifeTimeMode);

var mailhog = builder.AddContainer("MailHog", "mailhog/mailhog")
                     .WithLifetime(LifeTimeMode)
                     .WithEndpoint("smtp", config =>
                     {
                         config.TargetPort = 1025;
                         config.Port = 1025;
                     })
                     .WithEndpoint("ui", config =>
                     {
                         config.Protocol = System.Net.Sockets.ProtocolType.Tcp;
                         config.UriScheme = "http";
                         config.TargetPort = 8025;
                         config.Port = 8025;
                     });


Roundcube
       .WithEnvironment(env =>
       {
           env.EnvironmentVariables.Add("ROUNDCUBEMAIL_DEFAULT_HOST", EmailHostUrl);
           env.EnvironmentVariables.Add("ROUNDCUBEMAIL_SMTP_SERVER", EmailHostUrl);
           env.EnvironmentVariables.Add("ROUNDCUBEMAIL_SMTP_PORT", "587");
           env.EnvironmentVariables.Add("ROUNDCUBEMAIL_IMAP_PORT", "993");
           env.EnvironmentVariables.Add("ROUNDCUBEMAIL_DEFAULT_PORT", "993");
       })
       .WithEndpoint("webmail", config =>
       {
           config.Protocol = System.Net.Sockets.ProtocolType.Tcp;
           config.UriScheme = "http";
           config.TargetPort = 80;
           config.Port = 8081;
       })
       .WithLifetime(LifeTimeMode);

Keycloak.WithRealmImport(RealmImportPath)
        .WithArgs("--hostname=localhost")
        .WithArgs("--hostname-strict=false")
        .WithEnvironment(env =>
        {
            env.EnvironmentVariables.Add("KeycloakAdminApiClient", KeycloakApiClientSecret);
            env.EnvironmentVariables.Add("CCP.ServiceAccount", ServiceAccountSecret);
        })
        .WithOtlpExporter()
        .WithLifetime(LifeTimeMode);


RabbitMq.WithOtlpExporter()
    .WithLifetime(LifeTimeMode);

// Add AI Models
IResourceBuilder<OllamaModelResource> EmbeddingModel = Ollama.AddModel("embedding", "nomic-embed-text:latest");
IResourceBuilder<OllamaModelResource> QwenModel = Ollama.AddModel("qwen", "qwen2.5:1.5b");

// Add Databases
IResourceBuilder<PostgresDatabaseResource> EmailDB = Postgres.AddDatabase(name: "emaildb", databaseName: "emaildb");
IResourceBuilder<PostgresDatabaseResource> ChatDB = Postgres.AddDatabase(name: "chatDB", databaseName: "chatDB");
IResourceBuilder<PostgresDatabaseResource> MessagingDB = Postgres.AddDatabase(name: "MessagingDatabase", databaseName: "messagingdb");
IResourceBuilder<PostgresDatabaseResource> CustomerDB = Postgres.AddDatabase(name: "customerdb", databaseName: "customerdb");
IResourceBuilder<PostgresDatabaseResource> TicketDB = Postgres.AddDatabase(name: "ticketdb", databaseName: "ticketdb");

// Add Projects and their dependencies
IResourceBuilder<ProjectResource> IdentityService = builder.AddProject<Projects.IdentityService_API>("identityservice-api");
IResourceBuilder<ProjectResource> MessagingService = builder.AddProject<Projects.MessagingService_Api>("messagingservice-api");
IResourceBuilder<ProjectResource> ChatService = builder.AddProject<Projects.ChatService_Api>("chatservice-api");
IResourceBuilder<ProjectResource> TicketService = builder.AddProject<Projects.TicketService_Api>("ticketservice-api");
IResourceBuilder<ProjectResource> CustomerService = builder.AddProject<Projects.CustomerService_Api>("customerservice-api");
IResourceBuilder<ProjectResource> Gateway = builder.AddProject<Projects.Gateway_Api>("ccp-gateway");
IResourceBuilder<ProjectResource> CCPWebsite = builder.AddProject<Projects.CCP_Website>("ccp-website");
IResourceBuilder<ProjectResource> UI = builder.AddProject<Projects.CCP_UI>("ccp-ui");
IResourceBuilder<ProjectResource> EmailService = builder.AddProject<Projects.EmailService_API>("emailservice-api");
IResourceBuilder<ProjectResource> EmailWorkerService = builder.AddProject<Projects.EmailService_Worker_Host>("emailservice-worker-host");


IdentityService
    .WithReference(Keycloak)
    .WaitFor(Keycloak)
    .WithEnvironment(env =>
    {
        env.EnvironmentVariables.Add("KeycloakAdminApiClient", "KeycloakAdminApiClient");
        env.EnvironmentVariables.Add("KeycloakAdminApiClientSecret", KeycloakApiClientSecret);
    })
    .WithUrlForEndpoint("https", endpoint =>
    {
        endpoint.Url = "/swagger";
        endpoint.DisplayLocation = UrlDisplayLocation.SummaryAndDetails;
        endpoint.DisplayText = "API Swagger";
    })
    .WithOtlpExporter();

EmailService
    .WithReference(EmailDB)
    .WaitFor(EmailDB)
    .WithReference(CustomerService)
    .WaitFor(CustomerService)
    .WaitFor(Keycloak)
    .WithReference(Keycloak)
    .WithEndpoint("https", endpoint => endpoint.IsProxied = false)
    .WithUrlForEndpoint("https", endpoint =>
    {
        endpoint.Url = "/swagger";
        endpoint.DisplayLocation = UrlDisplayLocation.SummaryAndDetails;
        endpoint.DisplayText = "API Swagger";
    })
    .WithEnvironment(env =>
    {
        env.EnvironmentVariables.Add("emailWorkerServiceUsername", EmailWorkerServiceUsername);
        env.EnvironmentVariables.Add("emailWorkerServicePassword", EmailWorkerServicePassword);
        env.EnvironmentVariables.Add("emailHostUrl", EmailHostUrl);
        env.EnvironmentVariables.Add("SERVICE_ACCOUNT_SECRET", ServiceAccountSecret);
        env.EnvironmentVariables.Add("MAILCOW_API_URL", MAILCOW_API_URL);
        env.EnvironmentVariables.Add("MAILCOW_API_KEY", MAILCOW_API_KEY);
        env.EnvironmentVariables.Add("Encryption_Key", EncryptionKey);

    })
    .WaitFor(RabbitMq)
    .WithReference(RabbitMq)
    .WithOtlpExporter();


TicketService
    .WaitFor(Keycloak)
    .WaitFor(TicketDB)
    .WaitFor(RabbitMq)
    .WithReference(Keycloak)
    .WithReference(TicketDB)
    .WithReference(RabbitMq)
    .WithReference(EmailService)
    .WaitFor(EmailService)
    .WithUrlForEndpoint("https", endpoint =>
    {
        endpoint.Url = "/swagger";
        endpoint.DisplayLocation = UrlDisplayLocation.SummaryAndDetails;
        endpoint.DisplayText = "API Swagger";
    })
    .WithOtlpExporter();

MessagingService
    .WaitFor(Keycloak)
    .WaitFor(MessagingDB)
    .WaitFor(TicketService)
    .WithReference(Keycloak)
    .WithReference(MessagingDB)
    .WithReference(RabbitMq)
    .WithReference(TicketService)
    .WaitFor(RabbitMq)
    .WithEnvironment("CCP.ServiceAccount", ServiceAccountSecret)
    .WithUrlForEndpoint("https", endpoint =>
    {
        endpoint.Url = "/swagger";
        endpoint.DisplayLocation = UrlDisplayLocation.SummaryAndDetails;
        endpoint.DisplayText = "API Swagger";
    })
    .WithOtlpExporter();

CustomerService.WaitFor(Keycloak)
        .WithReference(Keycloak)
        .WaitFor(CustomerDB)
        .WithReference(CustomerDB)
        .WithOtlpExporter()
        .WithEnvironment(env =>
        {
            env.EnvironmentVariables.Add("Encryption_Key", EncryptionKey);
        })
        .WithUrlForEndpoint("https", (endpoint) =>
        {
            endpoint.Url = "/swagger";
            endpoint.DisplayLocation = UrlDisplayLocation.SummaryAndDetails;
            endpoint.DisplayText = "API Swagger";
        });

ChatService
    .WaitFor(IdentityService)
    .WaitFor(Keycloak)
    .WaitFor(ChatDB)
    .WaitFor(Ollama)
    .WaitFor(TicketService)
    .WaitFor(EmbeddingModel)
    .WaitFor(QwenModel)
    .WithReference(IdentityService)
    .WithReference(Keycloak)
    .WithReference(TicketService)
    .WithReference(ChatDB)
    .WithReference(Ollama)
    .WithReference(EmbeddingModel)
    .WithReference(QwenModel)
    .WithEnvironment(env =>
    {
        env.EnvironmentVariables.Add("SERVICE_ACCOUNT_SECRET", ServiceAccountSecret);
    })
    .WithOtlpExporter();


Gateway
    .WaitFor(Keycloak)
    .WaitFor(TicketService)
    .WaitFor(MessagingService)
    .WaitFor(IdentityService)
    .WithReference(Keycloak)
    .WithReference(TicketService)
    .WithReference(MessagingService)
    .WithReference(IdentityService)
    .WithEnvironment("CCP.ServiceAccount", ServiceAccountSecret)
    .WithUrlForEndpoint("https", endpoint =>
    {
        endpoint.Url = "/swagger";
        endpoint.DisplayLocation = UrlDisplayLocation.SummaryAndDetails;
        endpoint.DisplayText = "API Swagger";
    })
    .WithOtlpExporter();

UI.WaitFor(MessagingService)
  .WaitFor(Keycloak)
  .WaitFor(IdentityService)
  .WaitFor(CustomerService)
  .WaitFor(ChatService)
  .WaitFor(TicketService)
  .WaitFor(Gateway)
  .WithReference(MessagingService)
  .WithReference(Keycloak)
  .WithReference(ChatService)
  .WithReference(CustomerService)
  .WithReference(IdentityService)
  .WithReference(TicketService)
  .WithReference(Gateway)
  .WithEndpoint("https", endpoint => endpoint.IsProxied = false)
  .WithOtlpExporter();

CCPWebsite
    .WaitFor(UI)
    .WaitFor(Keycloak)
    .WaitFor(IdentityService)
    .WithReference(UI)
    .WithReference(IdentityService)
    .WithReference(Keycloak)
    .WithOtlpExporter();

EmailWorkerService
    .WithEnvironment(env =>
    {
        env.EnvironmentVariables.Add("emailWorkerServiceUsername", EmailWorkerServiceUsername);
        env.EnvironmentVariables.Add("emailWorkerServicePassword", EmailWorkerServicePassword);
        env.EnvironmentVariables.Add("emailHostUrl", EmailHostUrl);
        env.EnvironmentVariables.Add("SERVICE_ACCOUNT_SECRET", ServiceAccountSecret);
        env.EnvironmentVariables.Add("MAILCOW_API_URL", MAILCOW_API_URL);
        env.EnvironmentVariables.Add("MAILCOW_API_KEY", MAILCOW_API_KEY);
        env.EnvironmentVariables.Add("Encryption_Key", EncryptionKey);
    })
    .WaitFor(EmailDB)
    .WaitFor(RabbitMq)
    .WaitFor(Keycloak)
    .WaitFor(CustomerService)
    .WaitFor(TicketService)
    .WaitFor(MessagingService)
    .WithReference(EmailDB)
    .WithReference(RabbitMq)
    .WithReference(Keycloak)
    .WithReference(CustomerService)
    .WithReference(TicketService)
    .WithReference(MessagingService)
    .WithOtlpExporter();


if (Environment == "DEV")
{
    Ollama.WithOpenWebUI(c => c.WithLifetime(LifeTimeMode))
        .WithDataVolume()
        .WithGPUSupport();

    Postgres.WithPgWeb(c => c.WithLifetime(LifeTimeMode))
            .WithVolume("pgdata", "/var/lib/postgresql/data");

    Keycloak.WithVolume("keycloak_data", "/opt/keycloak/data");

    RabbitMq.WithDataVolume("rabbitmq_data").WithOtlpExporter().WithManagementPlugin(port: 15672);
}


builder.Build().Run();
