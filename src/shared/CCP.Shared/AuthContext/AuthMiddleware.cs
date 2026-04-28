using System.Security.Claims;
using CCP.Shared.ResultAbstraction;
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
                bool UserIsServiceAccount = IsServiceAccount(context);
                if (UserIsServiceAccount)
                {
                    var tenantIdResult = await GetTenantIdFromServiceAccount(context);
                    if (tenantIdResult.IsSuccess)
                        userContext.SetOrganizationId(tenantIdResult.Value);

                }
                else
                {
                    var userIdResult = await GetUserIdAsync(context);
                    if (userIdResult.IsSuccess)
                    {
                        userContext.SetCurrentUser(userIdResult.Value);
                    }
                    else
                    {
                        await _next(context);
                        return;
                    }

                    var tenantIdResult = await GetTenantIdAsync(context);
                    if (tenantIdResult.IsFailure)
                    {
                        await _next(context);
                        return;
                    }
                    else
                    {
                        userContext.SetOrganizationId(tenantIdResult.Value);
                    }

                    var orgNameResult = await GetOrgName(context);

                    if (orgNameResult.IsSuccess)
                    {
                        userContext.SetOrganizationName(orgNameResult.Value);
                    }
                    else
                    {
                        await _next(context);
                        return;
                    }
                }
            }
            await _next(context);
        }


        public async Task<Result<Guid>> GetTenantIdFromServiceAccount(HttpContext context)
        {
            var headers = context.Request.Headers;

            if (headers != null && headers.TryGetValue("X-Tenant-ID", out var tenantIdHeader))
            {
                if (Guid.TryParse(tenantIdHeader, out var tenantId) && tenantId != Guid.Empty)
                    return tenantId;
            }
            return Result.Failure<Guid>(Error.Failure("FailedToRetriveTenantId", "Could not get tenantID"));
        }

        public async Task<Result<Guid>> GetTenantIdAsync(HttpContext context)
        {
            var orgIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == "org")
                           ?? context.User.Claims.FirstOrDefault(c => c.Type == "organization");
            if (orgIdClaim != null)
            {
                Dictionary<string, Dictionary<string, string>>? orgData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(orgIdClaim.Value);
                if (orgData != null)
                {
                    if (orgData.First().Value.TryGetValue("id", out string? orgId))
                    {
                        return Result.Success(Guid.Parse(orgId));
                    }
                }
            }

            return Result.Failure<Guid>(Error.Failure("TenantIdNotFound", "Tenant ID not found in claims."));
        }

        public async Task<Result<Guid>> GetUserIdAsync(HttpContext context)
        {
            Claim? userIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim != null)
            {
                return Guid.Parse(userIdClaim.Value);
            }
            return Result.Failure<Guid>(Error.Failure(code: "FailedToGetUserId", "Could not find userid claim"));
        }


        public async Task<Result<string>> GetOrgName(HttpContext context)
        {
            try
            {
                var orgNameClaim = context.User.Claims.FirstOrDefault(c => c.Type == "organization")
                            ?? context.User.Claims.FirstOrDefault(c => c.Type == "organization_name");
                if (orgNameClaim != null)
                {
                    return orgNameClaim.Value;
                }
                return Result.Failure<string>(Error.Failure("ErrorGettingOrgName", "Error trying to get org name"));
            }
            catch (Exception)
            {
                return Result.Failure<string>(Error.Failure("ErrorGettingOrgName", "Error trying to get org name"));
            }
        }


        public bool IsServiceAccount(HttpContext context)
        {
            return context.User.Claims.Any(c => c.Type.ToLower() == "is_service_account");
        }

    }
}
