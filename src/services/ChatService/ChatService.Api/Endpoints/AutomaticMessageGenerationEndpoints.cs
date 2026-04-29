using CCP.Shared.ResultAbstraction;
using ChatService.Application.Services.Automated;
using ChatService.Domain.Entities.AI;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Api.Endpoints
{
    public static class AutomaticMessageGenerationEndpoints
    {
        public static IEndpointRouteBuilder MapAutomaticMessageGenerationEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var autoMessageGroup = endpoints.MapGroup("/AI")
                                            .WithTags("Automated Messages")
                                            .RequireAuthorization();

            autoMessageGroup.MapPost("/Generate", GenerateAutomatedMessage)
                .Produces<GeneratedReply>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status500InternalServerError)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status503ServiceUnavailable)
                .ProducesProblem(StatusCodes.Status401Unauthorized);

            return endpoints;
        }

        private static async Task<IResult> GenerateAutomatedMessage([FromServices] IAutomaticMessageGeneration automaticMessageGeneration, [FromQuery] int TicketId)
        {
            try
            {
                var result = await automaticMessageGeneration.GenerateReplyUsingAI(TicketId);
                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : result.ToProblemDetails();
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}
