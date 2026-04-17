using CCP.Shared.ResultAbstraction;
using ChatService.Application.Services.Session;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Api.Endpoints
{
    public static class SessionEndpoints
    {
        public static IEndpointRouteBuilder MapSessionEndpoints(this IEndpointRouteBuilder app)
        {
            var sessionRoute = app.MapGroup("/session")
                                  .WithTags("Sessions");


            sessionRoute.MapGet("/", GetSessions)
                        .WithName("GetSessions")
                        .WithTags("Sessions")
                        .RequireAuthorization();

            sessionRoute.MapPost("/", CreateSession)
                        .Produces<Guid>(StatusCodes.Status200OK)
                        .ProducesProblem(StatusCodes.Status400BadRequest)
                        .ProducesProblem(StatusCodes.Status401Unauthorized)
                        .WithName("CreateSession")
                        .WithTags("Sessions");
            return app;
        }

        private static async Task<IResult> CreateSession([FromServices] ISessionManagement sessionManagement, [FromServices] IHttpContextAccessor httpContextAccessor)
        {
            try
            {

                var domain = httpContextAccessor.HttpContext?.Request.Host.Host;
                if (domain == null) return Results.BadRequest("Domain is required.");
                var result = await sessionManagement.CreateSession(domain);
                if (result.IsSuccess)
                {
                    // Set the cookie with the session ID
                    httpContextAccessor.HttpContext?.Response.Cookies.Append(
                        "SessionId",
                        result.Value.ToString(), // assuming result.Value is the session ID (Guid)
                        new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.None,
                        }
                    );
                    return Results.Ok(result.Value);
                }
                else
                {
                    return result.ToProblemDetails();
                }
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }

        private static async Task<IResult> GetSessions()
        {
            return Results.Ok();
        }
    }
}
