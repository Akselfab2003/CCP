using CCP.Shared.ResultAbstraction;
using ChatService.Application.Services.Domain;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Api.Endpoints
{
    public static class ConfigurationEndpoints
    {
        public static IEndpointRouteBuilder MapConfigurationEndpoints(this IEndpointRouteBuilder routeBuilder)
        {
            var configurationGroup = routeBuilder.MapGroup("/configuration")
                                                 .RequireAuthorization();

            configurationGroup.MapPost("/AddDomain", AddDomain)
                              .WithName("AddDomain")
                              .WithTags("Configuration")
                              .Produces(StatusCodes.Status200OK)
                              .ProducesProblem(StatusCodes.Status400BadRequest)
                              .ProducesProblem(StatusCodes.Status401Unauthorized);

            return configurationGroup;
        }

        private static async Task<IResult> AddDomain([FromServices] IDomainServices domainServices, [FromQuery] string domain)
        {
            try
            {
                var result = await domainServices.AddOrUpdateDomainDetails(domain);
                return result.IsSuccess
                    ? Results.Ok()
                    : result.ToProblemDetails();
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}
