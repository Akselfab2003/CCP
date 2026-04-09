using System.Reflection;
using CCP.ServiceDefaults.Extensions;
using CCP.ServiceDefaults.Startup;
using CCP.ServiceDefaults.swagger;
using CCP.Shared.AuthContext;
using EmailService.Api.Services;
using EmailService.Application.Interfaces;
using EmailService.Application.Services;
using EmailService.Domain.Interfaces;
using EmailService.Infrastructure.Data;
using EmailService.Infrastructure.EmailInfrastructure;
using Microsoft.EntityFrameworkCore;

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
    builder.Services.AddDbContext<DBcontext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("EmailDB"));
    });
    builder.Services.AddApiAuthenticationServices("EmailService.Api", "CCP");
    builder.Services.AddOpenApi(op => OpenApiConfiguration.SetupOpenApiForSwagger(op));
    builder.Services.AddSwaggerGen(c => { SetupSwagger.SetupSwaggerForChatApp(c); });
}
builder.Services.AddScoped<IEmailReceived, EmailReceivedRepo>();
builder.Services.AddScoped<IEmailSent, EmailSentRepo>();
builder.Services.AddScoped<IEmail, EmailSendingService>();
builder.Services.AddScoped<ISmtpClient, SmtpClient>();
builder.Services.AddScoped<IEmailWorkerConfigurationRepo, TenantEmailConfigurationRepo>()
                .AddScoped<ITenantEmailConfigurationRepo, TenantEmailConfigurationRepo>();

builder.Services.AddScoped<ITenantEmailConfigurationService, TenantEmailConfigurationService>();

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
