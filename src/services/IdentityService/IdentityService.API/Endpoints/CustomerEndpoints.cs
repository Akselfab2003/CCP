using CCP.Shared.ResultAbstraction;
using IdentityService.Application.Models;
using IdentityService.Application.Services.Customer;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.API.Endpoints
{
    public static class CustomerEndpoints
    {
        public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder routeBuilder)
        {
            var customerRoute = routeBuilder.MapGroup("/customer")
                                            .WithTags("Customer")
                                            .RequireAuthorization();

            customerRoute.MapPost("/Invite", InviteCustomer)
                        .Produces(StatusCodes.Status200OK)
                        .ProducesProblem(StatusCodes.Status400BadRequest)
                        .ProducesProblem(StatusCodes.Status500InternalServerError);

            customerRoute.MapGet("/GetAllCustomers", GetAllCustomers)
                        .Produces<List<TenantMemberDto>>(StatusCodes.Status200OK)
                        .ProducesProblem(StatusCodes.Status400BadRequest)
                        .ProducesProblem(StatusCodes.Status500InternalServerError);

            return routeBuilder;
        }

        private static async Task<IResult> GetAllCustomers([FromServices] ICustomerService customerService)
        {
            try
            {
                Result<List<TenantMemberDto>> result = await customerService.GetAllTenantCustomerUsers();
                return result.IsSuccess
                                   ? Results.Ok(result.Value)
                                   : result.ToProblemDetails();
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }
        private static async Task<IResult> InviteCustomer([FromServices] ICustomerService customerService, string Email)
        {
            try
            {
                Result InviteResult = await customerService.InviteCustomer(Email);

                return InviteResult.IsSuccess
                                   ? Results.Ok()
                                   : InviteResult.ToProblemDetails();

            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }
    }
}
