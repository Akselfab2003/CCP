using Gateway.Api.Dtos;
using IdentityService.Sdk.Services.User;
using MessagingService.Sdk.Services;
using TicketService.Sdk.Services.Ticket;

namespace Gateway.Api.Endpoints
{
    public static class GatewayEndpoints
    {
        public static IEndpointRouteBuilder MapGatewayEndpoints(this IEndpointRouteBuilder builder)
        {
            var group = builder.MapGroup("/gateway")
                               .WithTags("Gateway")
                               .RequireAuthorization();

            group.MapGet("/tickets/{ticketId:int}/detail", GetTicketDetail)
                 .Produces<TicketDetailAggregateDto>(StatusCodes.Status200OK)
                 .ProducesProblem(StatusCodes.Status404NotFound)
                 .ProducesProblem(StatusCodes.Status500InternalServerError);

            group.MapGet("/dashboard/manager", GetManagerDashboard)
                 .Produces<ManagerDashboardAggregateDto>(StatusCodes.Status200OK)
                 .ProducesProblem(StatusCodes.Status500InternalServerError);

            return builder;
        }

        private static async Task<IResult> GetTicketDetail(
            int ticketId,
            ITicketService ticketService,
            IMessageSdkService messagingService,
            IUserService userService,
            ILogger<Program> logger,
            CancellationToken ct)
        {
            try
            {
                var ticketResult = await ticketService.GetTicket(ticketId, ct);
                if (!ticketResult.IsSuccess)
                    return ticketResult.ToProblemDetails();

                var ticket = ticketResult.Value;

                var messagesResult = await messagingService.GetMessagesByTicketIdAsync(ticketId, cancellationToken: ct);
                var messages = messagesResult.IsSuccess ? messagesResult.Value.Items.ToList() : new();

                var userIds = new HashSet<Guid>();
                if (ticket.CustomerId.HasValue) userIds.Add(ticket.CustomerId.Value);
                if (ticket.AssignedUserId.HasValue) userIds.Add(ticket.AssignedUserId.Value);
                foreach (var m in messages)
                    if (m.UserId.HasValue && m.UserId.Value != Guid.Empty)
                        userIds.Add(m.UserId.Value);

                var nameTasks = userIds.Select(async id =>
                {
                    try
                    {
                        var r = await userService.GetUserDetailsAsync(id, ct);
                        return (id, name: r.IsSuccess ? r.Value.name : (string?)null);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Could not resolve user name for {UserId}", id);
                        return (id, name: (string?)null);
                    }
                });

                var nameResults = await Task.WhenAll(nameTasks);
                var userNames = nameResults
                    .Where(r => r.name is not null)
                    .ToDictionary(r => r.id.ToString(), r => r.name!);

                return Results.Ok(new TicketDetailAggregateDto
                {
                    Ticket = ticket,
                    Messages = messages,
                    UserNames = userNames
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error building ticket detail aggregate for ticket {TicketId}", ticketId);
                return Results.Problem("An error occurred while building ticket detail.");
            }
        }

        private static async Task<IResult> GetManagerDashboard(
            ITicketService ticketService,
            IUserService userService,
            ILogger<Program> logger,
            CancellationToken ct)
        {
            try
            {
                var statsResult = await ticketService.GetManagerStatsAsync(ct);
                if (!statsResult.IsSuccess)
                    return statsResult.ToProblemDetails();

                var stats = statsResult.Value;

                var userIds = stats.TeamPerformance
                    .Select(p => p.UserId)
                    .Distinct()
                    .ToList();

                var nameTasks = userIds.Select(async id =>
                {
                    try
                    {
                        var r = await userService.GetUserDetailsAsync(id, ct);
                        return (id, name: r.IsSuccess ? r.Value.name : (string?)null);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Could not resolve user name for {UserId}", id);
                        return (id, name: (string?)null);
                    }
                });

                var nameResults = await Task.WhenAll(nameTasks);
                var userNames = nameResults
                    .Where(r => r.name is not null)
                    .ToDictionary(r => r.id.ToString(), r => r.name!);

                return Results.Ok(new ManagerDashboardAggregateDto
                {
                    Stats = stats,
                    UserNames = userNames
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error building manager dashboard aggregate");
                return Results.Problem("An error occurred while building manager dashboard.");
            }
        }
    }
}
