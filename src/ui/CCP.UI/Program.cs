using Gateway.Sdk.ServiceDefaults;
using CCP.ServiceDefaults;
using CCP.Shared.AuthContext;
using CCP.Shared.UIContext;
using CCP.Shared.ValueObjects;
using CCP.UI.Components;
using CCP.UI.Services;
using ChatService.Sdk.ServiceDefaults;
using IdentityService.Sdk.ServiceDefaults;
using MessagingService.Sdk.ServiceDefaults;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using TicketService.Sdk.ServiceDefaults;
using CustomerService.Sdk.ServiceDefaults;

namespace CCP.UI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddEnvironmentVariables();

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddHttpClient();
            builder.Services.AddCascadingAuthenticationState();

            // Trust proxy headers from Traefik
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownIPNetworks.Clear();
                options.KnownProxies.Clear();
            });


            var keycloakURL = builder.Configuration.GetValue<string>("services:Keycloak:http:0") ?? throw new InvalidOperationException("KeycloakServiceUrl configuration value is required.");
            var metadataAddress = builder.Configuration.GetValue<string>("services:Keycloak:metadataAddress") ?? $"{keycloakURL}/realms/CCP/.well-known/openid-configuration";


            if (builder.Configuration.GetValue<bool>("UI_TESTS", defaultValue: false))
            {
                builder.Services.AddAuthenticationCore();
            }
            else
            {
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
            .AddOpenIdConnect(options =>
            {
                options.Authority = $"{keycloakURL}/realms/CCP";
                options.MetadataAddress = metadataAddress;
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
                options.PushedAuthorizationBehavior = PushedAuthorizationBehavior.Disable;

                options.BackchannelHttpHandler = new HttpClientHandler
                {

                };

                if (builder.Environment.IsProduction())
                {
                    options.Events = new OpenIdConnectEvents
                    {
                        OnRedirectToIdentityProvider = context =>
                        {
                            context.ProtocolMessage.RedirectUri = "https://ccp.northflow.dev/signin-oidc";
                            return Task.CompletedTask;
                        }
                    };
                }
            });
            }

            //Authorization using direct role-based authorization on pages
            builder.Services.AddAuthorization();

            builder.Services.AddScoped<ChatHubService>();
            builder.Services.AddScoped<ICurrentUser, CurrentUser>();
            builder.Services.AddScoped<IUIUserContext, UIUserContext>();
            builder.Services.AddServiceDefaults("CCP.UI");
            builder.Services.AddMessageServiceSDK(
                builder.Configuration.GetValue<string>("services:messagingservice-api:http:0")
                ?? throw new InvalidOperationException("MessagingServiceUrl configuration value is required."));

            builder.Services.AddCustomerviceSdk(
                builder.Configuration.GetValue<string>("services:customerservice-api:http:0")
                ?? throw new InvalidOperationException("CustomerServiceUrl configuration value is required."));

            builder.Services.AddIdentityServiceSdk(
                builder.Configuration.GetValue<string>("services:identityservice-api:http:0")
                ?? throw new InvalidOperationException("IdentityServiceUrl configuration value is required."));

            builder.Services.AddTicketServiceSdk(
                builder.Configuration.GetValue<string>("services:ticketservice-api:http:0")
                ?? throw new InvalidOperationException("TicketServiceUrl configuration value is required.")
                );

            builder.Services.AddChatServiceSdk(
                builder.Configuration.GetValue<string>("services:chatservice-api:http:0")
                ?? throw new InvalidOperationException("ChatServiceUrl configuration value is required.")
                );

            builder.Services.AddGatewayServiceSdk(
                builder.Configuration.GetValue<string>("services:ccp-gateway:http:0")
                ?? throw new InvalidOperationException("GatewayServiceUrl configuration value is required.")
                );

            var app = builder.Build();

            app.UseForwardedHeaders(new ForwardedHeadersOptions()
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });


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

            app.MapGet("/authentication/logout", async (HttpContext context, IMemoryCache memoryCache) =>
            {
                // Evict the cached access token before signing out so that re-login always fetches a fresh token rather than serving the stale cached one.
                var sub = context.User.FindFirst("sub")?.Value
                       ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (sub is not null)
                    memoryCache.Remove($"user_token:{sub}:");

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
