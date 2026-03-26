
using System.Reflection;
using CCP.ServiceDefaults.Extensions;
using CCP.ServiceDefaults.Startup;
using CCP.ServiceDefaults.swagger;
using CCP.Shared.AuthContext;
using CustomerService.Api.DB;
using CustomerService.Application.ServiceCollection;
using CustomerService.Infrastructure.ServiceCollection;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            // Configure services
            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddAuthentication();
            builder.Services.AddAuthorization();
            builder.Services.ConfigureDefaultOpenTelemetry("CustomerService.Api");

            if (Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
            {
                builder.Services.AddOpenApi(op => OpenApiConfiguration.SetupOpenApiForSwagger(op));
                builder.Services.AddSwaggerGen(c => { SetupSwagger.SetupSwaggerForChatApp(c); });
                builder.Services.AddApiAuthenticationServices("CustomerService.Api", "CCP");
                builder.Services.AddApplicationServices();
                builder.Services.AddInfrastructureServices(builder.Configuration);
                builder.Services.AddDbContext<CustomerDBContext>(option =>
                {
                    option.UseNpgsql(builder.Configuration.GetConnectionString("customerdb"));
                });

            }



            var app = builder.Build();
            app.UseAuthentication();
            app.UseAuthorization();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            if (Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
            {
                AutomaticallyApplyDBMigration<CustomerDBContext>.ApplyMigrationsAsync(app).Wait();
                app.AppMapSwaggerExtensions();
                app.UseMiddleware<AuthMiddleware>();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
