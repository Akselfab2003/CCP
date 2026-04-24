using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CPP.UI.Tests.Utils
{
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public new const string Scheme = "Test";
        private readonly TestUserContext _userContext;

        public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
                               ILoggerFactory logger,
                               UrlEncoder encoder,
                               TestUserContext userContext) : base(options, logger, encoder)
        {
            _userContext = userContext;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (_userContext.User == null)
            {
                return Task.FromResult(AuthenticateResult.Fail("No User"));
            }

            var ticket = new AuthenticationTicket(_userContext.User, Scheme);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
