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
ContainerLifetime LifeTimeMode = Environment == "DEV" ? ContainerLifetime.Persistent : ContainerLifetime.Session;


// External Services
IResourceBuilder<OllamaResource> Ollama = builder.AddOllama("ollama");
IResourceBuilder<KeycloakResource> Keycloak = builder.AddKeycloak("keycloak", 8080);
IResourceBuilder<PostgresServerResource> Postgres = builder.AddPostgres("postgres");
IResourceBuilder<ContainerResource> DockerEmailServer = builder.AddContainer("MailServer", "mailserver/docker-mailserver");
IResourceBuilder<ContainerResource> Roundcube = builder.AddContainer("Roundcube", "roundcube/roundcubemail:latest");

// Configure External Services
Postgres.WithImage("pgvector/pgvector", "pg16")
        .WithBindMount("./init-db", "/docker-entrypoint-initdb.d")
        .WithLifetime(LifeTimeMode)
        .WithOtlpExporter();

Ollama.WithOtlpExporter()
      .WithLifetime(LifeTimeMode);

DockerEmailServer
    .WithEnvironment(env =>
    {
        env.EnvironmentVariables.Add("ENABLE_FAIL2BAN", "1");
        env.EnvironmentVariables.Add("PERMIT_DOCKER", "network");
        env.EnvironmentVariables.Add("SPOOF_PROTECTION", "0");
        env.EnvironmentVariables.Add("OVERRIDE_HOSTNAME", "mail.local");
    })
    .WithEndpoint("smtp", config =>
    {
        config.TargetPort = 25;
        config.Port = 25;
    })
    .WithEndpoint("submission", config =>
    {
        config.TargetPort = 587;
        config.Port = 587;
    })
    .WithEndpoint("smtps", config =>
    {
        config.TargetPort = 465;
        config.Port = 465;
    })
    .WithLifetime(LifeTimeMode);


Roundcube
       .WithEnvironment(env =>
       {
           env.EnvironmentVariables.Add("ROUNDCUBEMAIL_DEFAULT_HOST", "MailServer");
           env.EnvironmentVariables.Add("ROUNDCUBEMAIL_SMTP_SERVER", "MailServer");
           env.EnvironmentVariables.Add("ROUNDCUBEMAIL_SMTP_PORT", "587");
           env.EnvironmentVariables.Add("ROUNDCUBEMAIL_IMAP_PORT", "143");
           env.EnvironmentVariables.Add("ROUNDCUBEMAIL_DEFAULT_PORT", "143");
       })
       .WithEndpoint("webmail", config =>
       {
           config.Protocol = System.Net.Sockets.ProtocolType.Tcp;
           config.UriScheme = "http";
           config.TargetPort = 80;
           config.Port = 8081;
       })
       .WaitFor(DockerEmailServer)
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
IResourceBuilder<ProjectResource> CCPWebsite = builder.AddProject<Projects.CCP_Website>("ccp-website");
IResourceBuilder<ProjectResource> UI = builder.AddProject<Projects.CCP_UI>("ccp-ui");


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

TicketService
    .WithReference(Keycloak)
    .WaitFor(Keycloak)
    .WithReference(TicketDB)
    .WaitFor(TicketDB)
    .WithUrlForEndpoint("https", endpoint =>
    {
        endpoint.Url = "/swagger";
        endpoint.DisplayLocation = UrlDisplayLocation.SummaryAndDetails;
        endpoint.DisplayText = "API Swagger";
    })
    .WithOtlpExporter();

MessagingService.WaitFor(Keycloak)
    .WaitFor(MessagingDB)
    .WithReference(Keycloak)
    .WithReference(MessagingDB)
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
    .WaitFor(Keycloak)
    .WaitFor(ChatDB)
    .WaitFor(Ollama)
    .WaitFor(TicketService)
    .WithReference(Keycloak)
    .WithReference(TicketService)
    .WithReference(ChatDB)
    .WithReference(Ollama)
    .WithOtlpExporter();

UI
    .WaitFor(MessagingService)
    .WaitFor(Keycloak)
    .WaitFor(IdentityService)
    .WaitFor(CustomerService)
    .WaitFor(TicketService)
    .WithReference(MessagingService)
    .WithReference(Keycloak)
    .WithReference(CustomerService)
    .WithReference(IdentityService)
    .WithReference(TicketService)
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


if (Environment == "DEV")
{
    Ollama.WithOpenWebUI(c => c.WithLifetime(LifeTimeMode));

    Postgres.WithPgWeb(c => c.WithLifetime(LifeTimeMode))
            .WithVolume("pgdata", "/var/lib/postgresql/data");

    DockerEmailServer.WithVolume("dms_mail_data", "/var/mail")
                     .WithVolume("dms_mail_state", "/var/mail-state")
                     .WithVolume("dms_mail_logs", "/var/log/mail")
                     .WithVolume("dms_config", "/tmp/docker-mailserver")
                     .WithBindMount("/etc/localtime", "/etc/localtime", isReadOnly: true);

    Keycloak.WithVolume("keycloak_data", "/opt/keycloak/data");
}








builder.AddProject<Projects.EmailService_Api>("emailservice-api");








builder.AddProject<Projects.EmailService_Worker_Host>("emailservice-worker-host");








builder.Build().Run();
