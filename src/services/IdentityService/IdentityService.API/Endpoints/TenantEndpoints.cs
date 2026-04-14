using CCP.Shared.ResultAbstraction;
using IdentityService.Application.Models;
using IdentityService.Application.Services.Member;
using IdentityService.Application.Services.Organization;
using IdentityService.Application.Services.Tenant;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Endpoints
{
    public static class TenantEndpoints
    {
        public static IEndpointRouteBuilder MapTenantEndpoints(this IEndpointRouteBuilder routeBuilder)
        {
            var tenantRoute = routeBuilder.MapGroup("/tenant")
                                         .WithTags("Tenant");
            //   .RequireAuthorization();

            tenantRoute.MapPost("/create", CreateTenant)
                       .Produces(StatusCodes.Status200OK)
                       .ProducesProblem(StatusCodes.Status400BadRequest)
                       .ProducesProblem(StatusCodes.Status500InternalServerError);

            tenantRoute.MapGet("/members", GetAllTenantMembers)
                       .RequireAuthorization()
                       .Produces<List<TenantMemberDto>>(StatusCodes.Status200OK)
                       .ProducesProblem(StatusCodes.Status400BadRequest)
                       .ProducesProblem(StatusCodes.Status404NotFound)
                       .ProducesProblem(StatusCodes.Status500InternalServerError);

            tenantRoute.MapPost("/invite", InviteNewTenantMember)
                       .RequireAuthorization()
                       .Produces(StatusCodes.Status200OK)
                       .ProducesProblem(StatusCodes.Status400BadRequest)
                       .ProducesProblem(StatusCodes.Status500InternalServerError);

            return routeBuilder;
        }

        private static async Task<IResult> InviteNewTenantMember([FromServices] IOrganizationService organizationService, string Email)
        {
            try
            {
                var result = await organizationService.InviteNewUserToJoinOrganization(Email);

                return result.IsSuccess
                            ? Results.Ok()
                            : result.ToProblemDetails();
            }
            catch (Exception ex)
            {
                return Results.Problem("An error occurred while inviting a new tenant member: " + ex.Message);
            }
        }

        private static async Task<IResult> GetAllTenantMembers([FromServices] IMemberService memberService)
        {
            try
            {

                Result<List<TenantMemberDto>> result = await memberService.GetAllInternalUsers();

                return result.IsSuccess
                            ? Results.Ok(result.Value)
                            : result.ToProblemDetails();
            }
            catch (Exception ex)
            {
                return Results.Problem("An error occurred while retrieving tenant members: " + ex.Message);
            }
        }

        private static async Task<IResult> CreateTenant([FromBody] CreateTenantRequest createTenantRequest, [FromServices] ITenantService tenantService)
        {
            try
            {
                var result = await tenantService.CreateTenant(createTenantRequest);
                return result.IsSuccess
                    ? Results.Ok()
                    : result.ToProblemDetails();
            }
            catch (Exception ex)
            {
                return Results.Problem("An error occurred while creating the tenant: " + ex.Message);
            }
        }
    }
}
