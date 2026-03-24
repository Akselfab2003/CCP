using CCP.Shared.AuthContext;
using Microsoft.Extensions.DependencyInjection;

namespace CCP.ServiceDefaults.Extensions
{
    public static class AuthenticationExtensions
    {
        public static void AddApiAuthenticationServices(this IServiceCollection builder, string serviceName, string realm, string keycloak = "http://localhost:8080")
        {
            builder.AddAuthentication()
                   .AddKeycloakJwtBearer(serviceName: serviceName, realm: realm, configureOptions: options =>
                   {
                       options.RequireHttpsMetadata = false;
                       options.Audience = serviceName;
                       options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
                       {
                           ValidateIssuer = false,
                           ValidateAudience = false,
                           ValidAudiences = ["CCP"]
                       };
                       options.Authority = $"{keycloak}/realms/{realm}";
                       options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                       {
                           OnAuthenticationFailed = context =>
                           {
                               return Task.CompletedTask;
                           },
                           OnTokenValidated = context =>
                           {
                               return Task.CompletedTask;
                           },
                           OnChallenge = context =>
                           {
                               return Task.CompletedTask;
                           }
                       };
                   });

            builder.AddScoped<ICurrentUser, CurrentUser>();
        }
    }
}
