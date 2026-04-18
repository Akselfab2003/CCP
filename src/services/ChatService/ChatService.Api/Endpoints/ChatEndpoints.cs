using CCP.Shared.ResultAbstraction;
using ChatService.Application.Models;
using ChatService.Application.Services.Chat;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ChatService.Api.Endpoints
{
    public static class ChatEndpoints
    {
        public static IEndpointRouteBuilder MapChatEndpoints(this IEndpointRouteBuilder builder)
        {
            var chatGroup = builder.MapGroup("/chat")
                                    .WithTags("Chat");



            chatGroup.MapPost("/message", SendMessage)
                .Produces<string>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status401Unauthorized);

            chatGroup.MapPost("/test", testsignalR);


            return builder;
        }

        private static async Task testsignalR([FromServices] IHubContext<ChatHub.ChatHub> hubContext, [FromQuery] string session, [FromQuery] string domain, [FromQuery] string message)
        {
            await hubContext.Clients.Group($"{domain}:{session}").SendAsync("test", message);
        }

        private static async Task<IResult> SendMessage([FromServices] IChatManagementService chatManagement, [FromBody] ChatMessageRequest request)
        {
            try
            {
                var response = await chatManagement.GetChatResponseToMessage(request.Message, request.ConversationId);
                return response.IsSuccess ? Results.Ok(response.Value) : response.ToProblemDetails();
            }
            catch (Exception)
            {
                return Results.Problem("An error occurred while processing the chat message.", statusCode: 500);
            }
        }



    }
}
