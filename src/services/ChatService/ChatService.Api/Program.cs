using System.Reflection;
using CCP.ServiceDefaults;
using CCP.ServiceDefaults.Startup;
using CCP.ServiceDefaults.swagger;
using ChatService.Api.Endpoints;
using ChatService.Data;
using ChatService.Interfaces;
using ChatService.Models;
using ChatService.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Aspire service defaults — tilføjer service discovery, health checks, OTEL
builder.Services.AddServiceDefaults("ChatService.Api");

builder.Services.Configure<ChatOptions>(
    builder.Configuration.GetSection("Chat"));

// EF Core med pgvector via Aspire
builder.Services.AddDbContext<ChatDbContext>(opts =>
    opts.UseNpgsql(
        builder.Configuration.GetConnectionString("chatDB"),
        o =>
        {
            o.UseVector();
        }));

var ollamaUrl = builder.Configuration["services:ollama:https:0"]
    ?? builder.Configuration["services:ollama:http:0"]
    ?? "http://localhost:49234";

builder.Services.AddHttpClient<IEmbeddingService, EmbeddingService>(c =>
    c.BaseAddress = new Uri(ollamaUrl));

builder.Services.AddHttpClient<IChatService, ChatService.Repositories.ChatService>(c =>
    c.BaseAddress = new Uri(ollamaUrl));

// Og samme for TicketService:
var ticketUrl = builder.Configuration["services:ticketservice-api:https:0"]
    ?? builder.Configuration["services:ticketservice-api:http:0"]
    ?? "http://localhost:5001";

builder.Services.AddHttpClient<ITicketClient, TicketClient>(c =>
    c.BaseAddress = new Uri(ticketUrl));

builder.Services.AddScoped<IFaqRepository, FaqRepository>();
builder.Services.AddControllers();

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

app.MapControllers();
app.MapSessionEndpoints();
app.Run();
