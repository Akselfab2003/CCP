using CCP.Shared.ResultAbstraction;
using CCP.Shared.ValueObjects;
using IdentityService.Application.Models;
using IdentityService.Application.Services.UserRights;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Endpoints
{
    public static class UserRightsManagementEndpoints
    {
        public static IEndpointRouteBuilder MapUserRightsManagementEndpoints(this IEndpointRouteBuilder routeBuilder)
        {
            var userRightsRoute = routeBuilder.MapGroup("/userrights")
                                              .WithTags("User Rights Management")
                                              .RequireAuthorization();


            userRightsRoute.MapPost("/assign", AssignRoleToUser)
                          .Produces(StatusCodes.Status200OK)
                          .ProducesProblem(StatusCodes.Status400BadRequest)
                          .ProducesProblem(StatusCodes.Status404NotFound)
                          .ProducesProblem(StatusCodes.Status500InternalServerError);



            return routeBuilder;
        }

        private static async Task<IResult> AssignRoleToUser([FromServices] IUserRightsManagementService userRightsManagementService, [FromBody] AssignUserRightsRequest request, CancellationToken ct = default)
        {
            try
            {
                if (Enum.TryParse(request.Role, true, out UserRole role))
                {
                    return Results.BadRequest($"Invalid role: {request.Role}. Valid roles are: {string.Join(", ", Enum.GetNames(typeof(UserRole)))}");
                }

                var result = await userRightsManagementService.AssignUserRights(request.UserId, role.ToGroupName(), ct);

                return result.IsSuccess
                    ? Results.Ok($"User rights assigned successfully to user with ID: {request.UserId}")
                    : result.ToProblemDetails();
            }
            catch (Exception ex)
            {
                return Results.Problem(title: "An error occurred while assigning user rights.", statusCode: StatusCodes.Status500InternalServerError, detail: ex.Message);
            }
        }
    }
}
