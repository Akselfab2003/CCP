using CCP.Shared.ResultAbstraction;
using IdentityService.Application.Models;
using IdentityService.Application.Services.User;
using Keycloak.Sdk.Models;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Endpoints
{
    public static class UserEndpoints
    {
        public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder routeBuilder)
        {
            var userGroup = routeBuilder.MapGroup("/user")
                                        .WithTags("User")
                                        .RequireAuthorization();

            userGroup.MapPost("/authenticate", GenerateAuthenticationToken)
                     .AllowAnonymous()
                     .Produces<string>(StatusCodes.Status200OK)
                     .ProducesProblem(StatusCodes.Status400BadRequest)
                     .ProducesProblem(StatusCodes.Status500InternalServerError);

            userGroup.MapGet("/{userID:guid}", GetUserDetails)
                     .Produces<UserKeycloakAccount>(StatusCodes.Status200OK)
                     .ProducesProblem(StatusCodes.Status400BadRequest)
                     .ProducesProblem(StatusCodes.Status404NotFound)
                     .ProducesProblem(StatusCodes.Status500InternalServerError);


            userGroup.MapGet("/search", SearchUsers)
                     .Produces<List<UserKeycloakAccount>>(StatusCodes.Status200OK)
                     .ProducesProblem(StatusCodes.Status400BadRequest)
                     .ProducesProblem(StatusCodes.Status404NotFound)
                     .ProducesProblem(StatusCodes.Status500InternalServerError);

            userGroup.MapGet("/test", async (HttpContext context) =>
            {
                return await GetAuthDetails(context);
            });

            return routeBuilder;
        }

        private static async Task<IResult> GetAuthDetails(HttpContext context)
        {
            try
            {
                if (context.User.Identity?.IsAuthenticated != true)
                {
                    return Results.Unauthorized();
                }
                var authDetails = new
                {
                    UserName = context.User.Identity.Name,
                    Claims = context.User.Claims.Select(c => new { c.Type, c.Value }).ToList()
                };
                return Results.Ok(authDetails);
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: $"An error occurred while fetching authentication details: {ex.Message}", statusCode: 500);

            }
        }

        private static async Task<IResult> GenerateAuthenticationToken([FromBody] AuthenticatingRequest authenticationRequest, [FromServices] IUserService userService)
        {
            try
            {
                var result = await userService.Authenticate(authenticationRequest, CancellationToken.None);
                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : result.ToProblemDetails();
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: $"An error occurred during authentication for user '{authenticationRequest.UserName}': {ex.Message}", statusCode: 500);
            }
        }



        private static async Task<IResult> GetUserDetails([FromRoute] Guid userID, [FromServices] IUserService userService)
        {
            try
            {
                var result = await userService.GetUserDetails(userID, CancellationToken.None);
                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : result.ToProblemDetails();
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: $"An error occurred while fetching user details for user with ID {userID}: {ex.Message}", statusCode: 500);
            }
        }

        private static async Task<IResult> SearchUsers([FromQuery] string searchTerm, [FromServices] IUserService userService)
        {
            try
            {
                var result = await userService.SearchUsers(searchTerm, CancellationToken.None);
                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : result.ToProblemDetails();
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: $"An error occurred while searching for users with query '{searchTerm}': {ex.Message}", statusCode: 500);
            }
        }
    }
}
