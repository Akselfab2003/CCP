using System.Reflection;
using EmailService.Sdk.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using TicketService.Api.Endpoints;
using TicketService.Application.ServiceDefaults;
using TicketService.Infrastructure.Persistence;
using TicketService.Infrastructure.ServiceCollection;

namespace TicketService.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.Strict;
            });

            builder.Services.AddOpenApi(op => OpenApiConfiguration.SetupOpenApiForSwagger(op));

            builder.Services.AddAuthentication();
            builder.Services.AddAuthorization();
            builder.Services.AddHttpContextAccessor();
            builder.Services.ConfigureDefaultOpenTelemetry("TicketService.Api");
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddApiAuthenticationServices("TicketService.Api", "CCP");

            if (Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
            {


                builder.Services.AddEmailServiceSdk(
                    builder.Configuration.GetValue<string>("services:emailservice-api:http:0")
                    ?? throw new InvalidOperationException("EmailServiceUrl configuration value is required."));

                builder.Services.AddDbContext<TicketDbContext>(options =>
                {
                    options.UseNpgsql(builder.Configuration.GetConnectionString("TicketDb"));
                });

                // Keep this inside the guard — Swagger UI only needed at runtime
                builder.Services.AddSwaggerGen(c => { SetupSwagger.SetupSwaggerForChatApp(c); });
            }

            builder.Services.AddApplication();
            builder.Services.AddInfrastructure();

            builder.Services.AddHttpClient("MessagingService", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("services:messagingservice-api:http:0")!);
            });




            var app = builder.Build();

            app.UseAuthentication();
            app.UseAuthorization();

            if (Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
            {
                app.AppMapSwaggerExtensions();
                app.UseMiddleware<AuthMiddleware>();
                AutomaticallyApplyDBMigration<TicketDbContext>.ApplyMigrationsAsync(app).Wait();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();
            app.MapTicketEndpoint()
               .MapAssignmentEndpoints();

            app.Run();
        }
    }
}
