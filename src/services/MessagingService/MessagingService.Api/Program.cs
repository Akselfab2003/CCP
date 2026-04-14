using System.Reflection;
using MessagingService.Api.Hubs;
using MessagingService.Application.ServiceCollection;
using MessagingService.Infrastructure.Persistence;
using MessagingService.Infrastructure.ServiceCollection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.ConfigureDefaultOpenTelemetry("MessagingService.Api");
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options =>
{
    OpenApiConfiguration.SetupOpenApiForSwagger(options);
});


if (Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
{
    builder.Services.AddDbContext<MessagingDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("MessagingDatabase"),
                      o => o.UseVector()));

    builder.Services.AddSwaggerGen(c => { SetupSwagger.SetupSwaggerForChatApp(c); });
    builder.Services.AddApiAuthenticationServices("MessagingService.Api", "CCP");
}

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddHttpClient("TicketService", client =>
{
    var url = builder.Configuration.GetValue<string>("services:ticketservice-api:http:0");
    if (!string.IsNullOrEmpty(url))
        client.BaseAddress = new Uri(url);
});

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
if (Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
{
    AutomaticallyApplyDBMigration<MessagingDbContext>.ApplyMigrationsAsync(app).Wait();
    app.AppMapSwaggerExtensions();
    app.UseMiddleware<AuthMiddleware>();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapHub<ChatHub>("/chathub");

app.Run();

namespace TestProgramNameSpace
{
    public partial class Program
    {
    }
}
