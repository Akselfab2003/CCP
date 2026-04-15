using System.Reflection;
using MessagingService.Api.Hubs;
using MessagingService.Application.ServiceCollection;
using MessagingService.Infrastructure.Persistence;
using MessagingService.Infrastructure.ServiceCollection;
using Microsoft.EntityFrameworkCore;
using Wolverine;
using Wolverine.RabbitMQ;

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


    builder.UseWolverine(opts =>
    {
        opts.UseRabbitMq(builder.Configuration.GetConnectionString("RabbitMQ")!)
            .AutoProvision();

        opts.ListenToRabbitQueue("ticket.assignment.updated")
            .CircuitBreaker(c =>
            {
                c.FailurePercentageThreshold = 10;
                c.PauseTime = TimeSpan.FromMinutes(1);
            })
            .UseDurableInbox();
    });
}

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

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
