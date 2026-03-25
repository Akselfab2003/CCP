using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace CCP.Shared.AuthContext
{
    public class AuthMiddleware
    {

        private readonly RequestDelegate _next;
        public AuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // org: { "name":{ "id":"org_id"} }

        public async Task InvokeAsync(HttpContext context, ICurrentUser userContext)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                Claim? userIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim != null)
                {
                    userContext.SetCurrentUser(Guid.Parse(userIdClaim.Value));
                }
                var orgIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == "org")
                            ?? context.User.Claims.FirstOrDefault(c => c.Type == "organization");
                if (orgIdClaim != null)
                {
                    Dictionary<string, Dictionary<string, string>>? orgData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(orgIdClaim.Value);
                    if (orgData != null)
                    {
                        if (orgData.First().Value.TryGetValue("id", out string? orgId))
                        {
                            userContext.SetOrganizationId(Guid.Parse(orgId));
                        }
                    }
                }
            }
            await _next(context);
        }
    }
}
