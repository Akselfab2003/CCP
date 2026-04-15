using ChatService.Application.Services;
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
                        .WithName("CreateSession")
                        .WithTags("Sessions");
            return app;
        }

        private static async Task<IResult> CreateSession([FromServices] ISessionManagement sessionManagement, [FromServices] IHttpContextAccessor httpContextAccessor)
        {

            var domain = httpContextAccessor.HttpContext?.Request.Headers[""].ToString();
            var result = await sessionManagement.CreateSession(domain);
            if (result.IsSuccess)
            {
                return Results.Ok();
            }
            else
            {
                return Results.Problem(result.ErrorMessage);
            }
        }

        private static async Task<IResult> GetSessions()
        {
            return Results.Ok();
        }
    }
}
