using Microsoft.AspNetCore.Components.Authorization;

namespace CPP.UI.Tests.Fixtures.Application
{
    public class FakeAuthStateProvider : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var identity = new System.Security.Claims.ClaimsIdentity();
            var user = new System.Security.Claims.ClaimsPrincipal(identity);
            return Task.FromResult(new AuthenticationState(user));
        }
    }
}
