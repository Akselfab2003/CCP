using System.Reflection;
using CCP.ServiceDefaults;
using CCP.ServiceDefaults.Startup;
using CCP.ServiceDefaults.swagger;
using ChatService.Api.Endpoints;
using ChatService.Application.ServiceCollection;
using ChatService.Infrastructure.Persistence;
using ChatService.Infrastructure.ServiceCollection;
using IdentityService.Sdk.ServiceDefaults;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Aspire service defaults — tilføjer service discovery, health checks, OTEL
builder.Services.AddServiceDefaults("ChatService.Api");

// EF Core med pgvector via Aspire
builder.Services.AddDbContext<ChatDbContext>(opts => opts.UseNpgsql(builder.Configuration.GetConnectionString("chatDB"), o => { o.UseVector(); }));

var ollamaUrl = builder.Configuration["services:ollama:https:0"]
                ?? builder.Configuration["services:ollama:http:0"]
                ?? "http://localhost:49234";


// Og samme for TicketService:
var ticketUrl = builder.Configuration["services:ticketservice-api:https:0"]
                ?? builder.Configuration["services:ticketservice-api:http:0"]
                ?? "http://localhost:5001";

builder.Services.AddIdentityServiceSdk(builder.Configuration.GetValue<string>("services:identityservice-api:http:0")
                                       ?? throw new InvalidOperationException("IdentityServiceUrl configuration value is required."));

builder.Services.AddControllers();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
// Swagger til debugging
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
{
    app.AppMapSwaggerExtensions();
    AutomaticallyApplyDBMigration<ChatDbContext>.ApplyMigrationsAsync(app).Wait();
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapSessionEndpoints();
app.Run();
