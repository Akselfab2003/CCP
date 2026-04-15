using System.Reflection;
using Microsoft.EntityFrameworkCore;
using TicketService.Api.Endpoints;
using TicketService.Application.ServiceDefaults;
using TicketService.Infrastructure.Persistence;
using TicketService.Infrastructure.ServiceCollection;
using Wolverine;
using Wolverine.RabbitMQ;

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
                builder.Services.AddDbContext<TicketDbContext>(options =>
                {
                    options.UseNpgsql(builder.Configuration.GetConnectionString("TicketDb"));
                });

                // Keep this inside the guard — Swagger UI only needed at runtime
                builder.Services.AddSwaggerGen(c => { SetupSwagger.SetupSwaggerForChatApp(c); });

                builder.UseWolverine(opts =>
                {
                    opts.UseRabbitMq(builder.Configuration.GetConnectionString("RabbitMQ")!)
                        .AutoProvision();

                    opts.PublishAllMessages().ToRabbitQueue("ticket.assignment.updated").UseDurableOutbox();
                });
            }

            builder.Services.AddApplication();
            builder.Services.AddInfrastructure();

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
