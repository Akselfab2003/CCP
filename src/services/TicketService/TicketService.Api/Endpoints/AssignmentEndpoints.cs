using Microsoft.AspNetCore.Mvc;
using TicketService.Api.Extensions;
using TicketService.Application.Services.Assignment;
using TicketService.Domain.RequestObjects;

namespace TicketService.Api.Endpoints
{
    public static class AssignmentEndpoints
    {
        public static IEndpointRouteBuilder MapAssignmentEndpoints(this IEndpointRouteBuilder builder)
        {
            var assignmentRoute = builder.MapGroup("/assignment")
                                         .WithTags("Assignment")
                                         .RequireAuthorization();

            assignmentRoute.MapPost("/assign", AssignTicket)
                           .Produces(StatusCodes.Status200OK)
                           .ProducesProblem(StatusCodes.Status404NotFound)
                           .ProducesProblem(StatusCodes.Status400BadRequest)
                           .ProducesProblem(StatusCodes.Status401Unauthorized)
                           .ProducesProblem(StatusCodes.Status500InternalServerError);


            return builder;
        }

        private static async Task<IResult> AssignTicket([FromServices] IAssignmentCommands assignmentCommands, [FromBody] UpdateTicketAssignmentRequest updateTicketAssignmentRequest)
        {
            try
            {
                var result = await assignmentCommands.CreateOrUpdateAssignment(updateTicketAssignmentRequest.TicketId, updateTicketAssignmentRequest.AssignToUserId);

                return result.IsSuccess
                    ? Results.Ok()
                    : result.ToProblemDetails();

            }
            catch (Exception)
            {
                return Results.Problem("An error occurred while assigning the ticket.");
            }
        }
    }
}
