using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CCP.ServiceDefaults.swagger
{
    public static class SetupSwagger
    {
        public static void SetupSwaggerForChatApp(this SwaggerGenOptions options)
        {
            options.SwaggerDoc("v1", new() { Title = "My API", Version = "v1" });

            // Add JWT Authentication to Swagger
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter 'Bearer' [space] and then your valid token."
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("bearer", document)] = new List<string>()
            });

            options.OperationFilter<AuthorizeOperationFilter>();
            options.OperationFilter<RoleOperationFilter>();
        }
    }
}
