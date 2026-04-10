using Microsoft.AspNetCore.Mvc;
using TicketService.Application.Services.Ticket;
using TicketService.Domain.RequestObjects;
using TicketService.Domain.ResponseObjects;

namespace TicketService.Api.Endpoints
{
    public static class TicketEndpoint
    {
        public static IEndpointRouteBuilder MapTicketEndpoint(this IEndpointRouteBuilder builder)
        {
            var ticketRoute = builder.MapGroup("/ticket")
                                     .WithTags("Ticket")
                                     .RequireAuthorization();

            ticketRoute.MapPost("/create", CreateTicket)
                       .Produces(StatusCodes.Status200OK)
                       .ProducesProblem(StatusCodes.Status400BadRequest)
                       .ProducesProblem(StatusCodes.Status500InternalServerError);

            ticketRoute.MapGet("/GetTicket/{ticketId:int}", GetTicketById)
                       .Produces<TicketDto>(StatusCodes.Status200OK)
                       .ProducesProblem(StatusCodes.Status404NotFound)
                       .ProducesProblem(StatusCodes.Status400BadRequest)
                       .ProducesProblem(StatusCodes.Status500InternalServerError);

            ticketRoute.MapGet("/GetTickets", GetTicketsByParameters)
                       .Produces<List<TicketDto>>(StatusCodes.Status200OK)
                       .ProducesProblem(StatusCodes.Status400BadRequest)
                       .ProducesProblem(StatusCodes.Status500InternalServerError);

            return builder;
        }

        private static async Task<IResult> GetTicketsByParameters([FromServices] ITicketQueries ticketQueries, [FromQuery] Guid? assignedUserId, [FromQuery] Guid? customerId, [FromQuery] string? TicketStatus)
        {
            try
            {
                var result = await ticketQueries.GetTicketsBasedOnParameters(assignedUserId, customerId, string.IsNullOrEmpty(TicketStatus) ? null : Enum.Parse<TicketStatus>(TicketStatus, true));
                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : result.ToProblemDetails();
            }
            catch (Exception ex)
            {
                return Results.Problem("An error occurred while retrieving tickets based on the provided parameters: " + ex.Message);
            }
        }

        private static async Task<IResult> GetTicketById([FromServices] ITicketQueries ticketQueries, [FromRoute] int ticketId)
        {
            try
            {
                var result = await ticketQueries.GetTicket(ticketId);
                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : result.ToProblemDetails();
            }
            catch (Exception ex)
            {
                return Results.Problem("An error occurred while retrieving the ticket: " + ex.Message);
            }
        }


        private static async Task<IResult> CreateTicket([FromServices] ITicketCommands ticketCommands, [FromBody] CreateTicketRequest request)
        {
            try
            {
                var result = await ticketCommands.CreateTicketAsync(request);
                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : result.ToProblemDetails();
            }
            catch (Exception ex)
            {
                return Results.Problem("An error occurred while creating the ticket: " + ex.Message);
            }
        }
    }
}
