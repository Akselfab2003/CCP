using CCP.ServiceDefaults;
using CCP.Shared.AuthContext;
using CCP.Shared.UIContext;
using CCP.UI.Components;
using CCP.UI.Services;
using IdentityService.Sdk.ServiceDefaults;
using MessagingService.Sdk.ServiceDefaults;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using TicketService.Sdk.ServiceDefaults;

namespace CCP.UI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddHttpClient();
            builder.Services.AddCascadingAuthenticationState();

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

            builder.Services.AddScoped<ChatHubService>();
            builder.Services.AddScoped<ICurrentUser, CurrentUser>();
            builder.Services.AddScoped<IUIUserContext, UIUserContext>();
            builder.Services.AddServiceDefaults("CCP.UI");
            /*
            builder.Services.AddEmailServiceSdk(
                builder.Configuration.GetValue<string>("services:EmailService:http:0")
                ?? throw new InvalidOperationException("EmailServiceUrl configuration value is required."));
            */
            builder.Services.AddMessageServiceSDK(
                builder.Configuration.GetValue<string>("services:messagingservice-api:http:0")
                ?? throw new InvalidOperationException("MessagingServiceUrl configuration value is required."));
            /*
            builder.Services.AddCustomerviceSdk(
                builder.Configuration.GetValue<string>("services:customerservice-api:http:0")
                ?? throw new InvalidOperationException("CustomerServiceUrl configuration value is required."));
            */
            builder.Services.AddIdentityServiceSdk(
                builder.Configuration.GetValue<string>("services:identityservice-api:http:0")
                ?? throw new InvalidOperationException("IdentityServiceUrl configuration value is required."));

            builder.Services.AddTicketServiceSdk(
                builder.Configuration.GetValue<string>("services:ticketservice-api:http:0")
                ?? throw new InvalidOperationException("TicketServiceUrl configuration value is required.")
                );

            var app = builder.Build();

            app.UseAuthentication();
            app.UseAuthorization();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseAntiforgery();
            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.MapGet("/authentication/login", (string? returnUrl) =>
            {
                IResult loginChallenged = TypedResults.Challenge(new AuthenticationProperties()
                {
                    RedirectUri = returnUrl ?? "/"
                });

                return loginChallenged;
            });

            app.MapGet("/authentication/logout", async (HttpContext context) =>
            {
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties
                {
                    RedirectUri = "/"
                });
            });

            app.Run();
        }
    }
}
