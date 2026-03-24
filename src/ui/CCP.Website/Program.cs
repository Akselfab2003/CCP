using CCP.Website.Components;
using CCP.Website.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace CCP.Website
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();


            var keycloakURL = builder.Configuration.GetValue<string>("services:Keycloak:http:0") ?? throw new InvalidOperationException("KeycloakServiceUrl configuration value is required.");



            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignOutScheme = OpenIdConnectDefaults.AuthenticationScheme;
            }).AddCookie(options =>
            {
                options.SlidingExpiration = false;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddKeycloakOpenIdConnect(serviceName: "Keycloak", realm: "CCP", options =>
            {
                options.Authority = $"{keycloakURL}/realms/CCP";
                options.ClientId = "CCP";
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.RequireHttpsMetadata = false;
                options.SaveTokens = true;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuer = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
                options.GetClaimsFromUserInfoEndpoint = true;
                options.SignedOutCallbackPath = "/signout-callback-oidc";
                options.SignedOutRedirectUri = keycloakURL;
                options.SignOutScheme = OpenIdConnectDefaults.AuthenticationScheme;
            });


            var SassServiceUrl = builder.Configuration.GetValue<string>("services:chatapp-ui:https:0");
            builder.Services.AddScoped<WebsiteReferencesService>(_ => new WebsiteReferencesService(SassServiceUrl ?? throw new InvalidOperationException("ChatAppUIUrl configuration value is required.")));
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddFluentUIComponents();





            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();
        }
    }
}
