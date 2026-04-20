using Microsoft.AspNetCore.Mvc;
using TicketService.Application.Services.Ticket;
using TicketService.Domain.Entities;
using TicketService.Domain.Interfaces;
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
                       .Produces<int>(StatusCodes.Status200OK)
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

            ticketRoute.MapPatch("/{ticketId:int}/status", UpdateTicketStatus)
                       .Produces(StatusCodes.Status200OK)
                       .ProducesProblem(StatusCodes.Status404NotFound)
                       .ProducesProblem(StatusCodes.Status400BadRequest)
                       .ProducesProblem(StatusCodes.Status401Unauthorized)
                       .ProducesProblem(StatusCodes.Status500InternalServerError);

            var historyRoute = builder.MapGroup("/ticket")
                                      .WithTags("Ticket")
                                      .RequireAuthorization();

            historyRoute.MapGet("/history/customer/{customerId:guid}", GetCustomerHistory)
                        .Produces<List<TicketHistoryEntry>>(StatusCodes.Status200OK)
                        .ProducesProblem(StatusCodes.Status500InternalServerError);

            historyRoute.MapPost("/{ticketId:int}/history/message", RecordMessageSent)
                        .Produces(StatusCodes.Status200OK)
                        .ProducesProblem(StatusCodes.Status400BadRequest)
                        .ProducesProblem(StatusCodes.Status500InternalServerError);

            ticketRoute.MapGet("/manager-stats", GetManagerStats)
                       .Produces<ManagerStatsDto>(StatusCodes.Status200OK)
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


        private static async Task<IResult> UpdateTicketStatus(
            [FromServices] ITicketCommands ticketCommands,
            [FromRoute] int ticketId,
            [FromBody] UpdateTicketStatusRequest request)
        {
            try
            {
                var result = await ticketCommands.UpdateTicketStatusAsync(ticketId, request.NewStatus);
                return result.IsSuccess ? Results.Ok() : result.ToProblemDetails();
            }
            catch (Exception)
            {
                return Results.Problem("An error occurred while updating the ticket status.");
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

        private static async Task<IResult> GetCustomerHistory(
            [FromServices] ITicketHistoryRepository historyRepository,
            [FromRoute] Guid customerId,
            [FromQuery] int limit = 20)
        {
            try
            {
                var entries = await historyRepository.GetByCustomerIdAsync(customerId, limit);
                return Results.Ok(entries);
            }
            catch (Exception ex)
            {
                return Results.Problem("An error occurred while retrieving customer history: " + ex.Message);
            }
        }

        private static async Task<IResult> GetManagerStats(
            [FromServices] IManagerStatsQuery statsQuery,
            [FromServices] ICurrentUser currentUser)
        {
            try
            {
                var stats = await statsQuery.GetManagerStatsAsync(currentUser.UserId);
                return Results.Ok(stats);
            }
            catch (Exception ex)
            {
                return Results.Problem("An error occurred while retrieving manager stats: " + ex.Message);
            }
        }

        private static async Task<IResult> RecordMessageSent(
            [FromServices] ITicketCommands ticketCommands,
            [FromRoute] int ticketId,
            [FromBody] RecordMessageSentRequest request)
        {
            try
            {
                var result = await ticketCommands.RecordMessageSentAsync(ticketId, request.SenderUserId, request.MessageSnippet);
                return result.IsSuccess ? Results.Ok() : result.ToProblemDetails();
            }
            catch (Exception ex)
            {
                return Results.Problem("An error occurred while recording message history: " + ex.Message);
            }
        }
    }
}
