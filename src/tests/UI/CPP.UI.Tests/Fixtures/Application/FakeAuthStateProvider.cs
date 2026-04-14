using System.Security.Claims;
using System.Text.Encodings.Web;
using CCP.Shared.ValueObjects;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CPP.UI.Tests.Fixtures.Application
{
    public class FakeAuthStateProvider : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public new const string Scheme = "Test";

        public FakeAuthStateProvider(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Role, UserRole.Customer.ToRoleString()) // optional
        };

            var identity = new ClaimsIdentity(claims, Scheme);
            var principal = new ClaimsPrincipal(identity);

            var ticket = new AuthenticationTicket(principal, Scheme);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
