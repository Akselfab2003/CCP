using CCP.Shared.ResultAbstraction;
using IdentityService.Application.Models;
using IdentityService.Application.Services.Member;
using IdentityService.Application.Services.Supporter;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Endpoints
{
    /// <summary>
    /// API endpoints til supporter operationer
    /// </summary>
    public static class SupporterEndpoints
    {
        /// <summary>
        /// Registrerer supporter endpoints i routing
        /// </summary>
        public static IEndpointRouteBuilder MapSupporterEndpoints(this IEndpointRouteBuilder routeBuilder)
        {
            var supporterRoute = routeBuilder.MapGroup("/supporter")
                                            .WithTags("Supporter")
                                            .RequireAuthorization();

            supporterRoute.MapPost("/Invite", InviteSupporter)
                        .Produces(StatusCodes.Status200OK)
                        .ProducesProblem(StatusCodes.Status400BadRequest)
                        .ProducesProblem(StatusCodes.Status404NotFound)
                        .ProducesProblem(StatusCodes.Status500InternalServerError);

            supporterRoute.MapGet("/GetAllSupporters", GetAllSupporters)
                        .Produces<List<TenantMemberDto>>(StatusCodes.Status200OK)
                        .ProducesProblem(StatusCodes.Status400BadRequest)
                        .ProducesProblem(StatusCodes.Status500InternalServerError);

            return routeBuilder;
        }

        /// <summary>
        /// POST /supporter/Invite?email={email}
        /// Sender invitation til en ny supporter
        /// </summary>
        private static async Task<IResult> InviteSupporter(
            [FromServices] ISupporterService supporterService,
            [FromQuery] string email)
        {
            try
            {
                Result inviteResult = await supporterService.InviteSupporter(email);

                return inviteResult.IsSuccess
                    ? Results.Ok()
                    : inviteResult.ToProblemDetails();
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }

        /// <summary>
        /// GET /supporter/GetAllSupporters
        /// Henter alle supporters i den nuværende organisation
        /// </summary>
        private static async Task<IResult> GetAllSupporters(
            [FromServices] IMemberService memberService)
        {
            try
            {
                Result<List<TenantMemberDto>> result = await memberService.GetAllSupportUsersOfTenant();
                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : result.ToProblemDetails();
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }

    }
}
