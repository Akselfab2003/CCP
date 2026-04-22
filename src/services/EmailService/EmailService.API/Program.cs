using System.Reflection;
using CCP.ServiceDefaults.Extensions;
using CCP.ServiceDefaults.Startup;
using CCP.ServiceDefaults.swagger;
using CCP.Shared.AuthContext;
using ChatApp.Encryption;
using CustomerService.Sdk.ServiceDefaults;
using Duende.AccessTokenManagement;
using Duende.IdentityModel.Client;
using EmailService.Application.Interfaces;
using EmailService.Application.Services;
using EmailService.Domain.Interfaces;
using EmailService.Infrastructure.Data;
using EmailService.Infrastructure.EmailInfrastructure;
using EmailTemplates.Renderes;
using MailCow.Sdk.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.Strict;
});


// Add Razor Components services for email template rendering
builder.Services.AddRazorComponents();
builder.Services.AddScoped<EmailTemplateRenderer>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureDefaultOpenTelemetry("EmailService.Api");

if (Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
{
    var keycloakServiceUrl =
        builder.Configuration.GetValue<string>("services:Keycloak:http:0")
        ?? throw new InvalidOperationException("KeycloakServiceUrl configuration value is required.");


    builder.Services.AddClientCredentialsTokenManagement()
                        .AddClient(ClientCredentialsClientName.Parse("CCP.ServiceAccount"), client =>
                        {
                            client.TokenEndpoint = new Uri($"{keycloakServiceUrl}/realms/CCP/protocol/openid-connect/token");
                            client.ClientId = ClientId.Parse("CCP.ServiceAccount");
                            client.ClientSecret = ClientSecret.Parse(
                                builder.Configuration["SERVICE_ACCOUNT_SECRET"]
                                ?? throw new InvalidOperationException("SERVICE_ACCOUNT_SECRET configuration value is required.")
                            );
                            client.Scope = Scope.ParseOrDefault("openid");
                            client.ClientCredentialStyle = ClientCredentialStyle.AuthorizationHeader;
                        });

    builder.Services.AddCustomerviceSdk(
        builder.Configuration.GetValue<string>("services:customerservice-api:http:0")
        ?? throw new InvalidOperationException("CustomerServiceUrl configuration value is required."), true);

    builder.Services.AddDbContext<DBcontext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("EmailDB"));
    });

    builder.Services.AddApiAuthenticationServices("EmailService.Api", "CCP", keycloakServiceUrl);
    builder.Services.AddOpenApi(op => OpenApiConfiguration.SetupOpenApiForSwagger(op));
    builder.Services.AddSwaggerGen(c => { SetupSwagger.SetupSwaggerForChatApp(c); });
    builder.Services.AddScoped<IQueuePublisherService, QueuePublisherService>();

    builder.UseWolverine(opts =>
    {
        opts.UseRabbitMq(builder.Configuration.GetConnectionString("RabbitMQ")!)
            .AutoProvision();

        opts.PublishAllMessages().ToRabbitQueue("mailbox.queue").UseDurableOutbox();
    });


    var mailCowApiUrl = builder.Configuration.GetValue<string>("MAILCOW_API_URL")
        ?? throw new InvalidOperationException("MAILCOW_API_URL configuration value is required.");

    var mailCowApiKey = builder.Configuration.GetValue<string>("MAILCOW_API_KEY")
        ?? throw new InvalidOperationException("MAILCOW_API_KEY configuration value is required.");
    builder.Services.AddMailCowSdk(mailCowApiUrl, mailCowApiKey);

    var encryptionKey = builder.Configuration["Encryption_Key"]
        ?? throw new InvalidOperationException("Encryption_Key configuration value is required.");
    builder.Services.AddSingleton<IEncryptionService>(new AesEncryptionService(encryptionKey));
}

builder.Services.AddScoped<IEmailReceived, EmailReceivedRepo>();
builder.Services.AddScoped<IEmailSent, EmailSentRepo>();
builder.Services.AddScoped<IEmail, EmailSendingService>();
builder.Services.AddScoped<ISmtpClient, SmtpClient>();
builder.Services.AddScoped<IEmailWorkerConfigurationRepo, TenantEmailConfigurationRepo>();
builder.Services.AddScoped<ITenantEmailConfigurationRepo, TenantEmailConfigurationRepo>();
builder.Services.AddScoped<ITicketEmailService, TicketEmailService>();
builder.Services.AddScoped<IEmailTemplateRenderer, EmailTemplateRenderer>();
builder.Services.AddScoped<ITenantEmailConfigurationService, TenantEmailConfigurationService>();
builder.Services.AddScoped<IEmailTicketEntitiesRepository, EmailTicketEntitiesRepository>();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<AuthMiddleware>();


if (Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
{
    AutomaticallyApplyDBMigration<DBcontext>.ApplyMigrationsAsync(app).Wait();
    app.AppMapSwaggerExtensions();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
