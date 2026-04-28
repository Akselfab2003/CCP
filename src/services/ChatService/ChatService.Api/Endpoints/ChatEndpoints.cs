using System.Reflection;
using CCP.Shared.ResultAbstraction;
using ChatService.Application.AuthContext;
using ChatService.Application.Models;
using ChatService.Application.Services.Chat;
using ChatService.Application.Services.Domain;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Api.Endpoints
{
    public static class ChatEndpoints
    {
        public static IEndpointRouteBuilder MapChatEndpoints(this IEndpointRouteBuilder builder)
        {
            var chatGroup = builder.MapGroup("/chat")
                                    .WithTags("Chat");


            if (Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
            {
                chatGroup.MapPost("/createConversation", CreateConversation)
                        .Produces<string>(StatusCodes.Status200OK)
                        .ProducesProblem(StatusCodes.Status400BadRequest)
                        .ProducesProblem(StatusCodes.Status401Unauthorized)
                        .RequireCors(c =>
                        {
                            c.SetIsOriginAllowed(origin =>
                            {
                                using var scope = builder.ServiceProvider.CreateScope();
                                var domainservices = scope.ServiceProvider.GetRequiredService<IDomainServices>();
                                var host = new Uri(origin).Host;
                                return domainservices.IsDomainAllowed(host);
                            })
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                        });
            }
            else
            {
                chatGroup.MapPost("/createConversation", CreateConversation)
                    .Produces<string>(StatusCodes.Status200OK)
                    .ProducesProblem(StatusCodes.Status400BadRequest)
                    .ProducesProblem(StatusCodes.Status401Unauthorized);
            }
            chatGroup.MapPost("/message", SendMessage)
                .Produces<string>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized);


            return builder;
        }

        private static async Task<IResult> CreateConversation([FromServices] IChatManagementService chatManagement, [FromServices] IActiveSession activeSession, [FromServices] IAuthParser authParser, [FromServices] IHttpContextAccessor httpContextAccessor)
        {
            try
            {
                var authControl = await authParser.ParseContext(httpContextAccessor.HttpContext!);
                if (authControl.IsFailure) return authControl.ToProblemDetails();

                var response = await chatManagement.CreateConversation(activeSession.SessionId);
                return response.IsSuccess ? Results.Ok(new { ConversationID = response.Value }) : response.ToProblemDetails();
            }
            catch (Exception)
            {
                return Results.Problem("An error occurred while creating the conversation.", statusCode: 500);
            }
        }
        private static async Task<IResult> SendMessage([FromServices] IChatManagementService chatManagement, [FromBody] ChatMessageRequest request)
        {
            try
            {
                var response = await chatManagement.SendMessageFromSupportToConversation(request.TicketId, request.Message);
                return response.IsSuccess
                               ? Results.Ok()
                               : response.ToProblemDetails();
            }
            catch (Exception)
            {
                return Results.Problem("An error occurred while processing the chat message.", statusCode: 500);
            }
        }



    }
}
