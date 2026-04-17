using CCP.Shared.ResultAbstraction;
using ChatService.Application.Models;
using ChatService.Application.Services.Faq;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Api.Endpoints
{
    public static class FaqManagementEndpoints
    {
        public static IEndpointRouteBuilder MapFaqManagementEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var faqGroup = endpoints.MapGroup("/faqs")
                                    .WithTags("FAQs")
                                    .RequireAuthorization();

            faqGroup.MapPost("/", CreateFaqEmbedding)
                .WithDisplayName("Create FAQ Embedding")
                .WithDescription("Creates an embedding for a sample FAQ and returns it. In a real application, you would typically save the embedding to a database.");

            return endpoints;
        }

        private static async Task<IResult> CreateFaqEmbedding([FromServices] IFaqManagementService faqManagementService, [FromBody] CreateFaqRequest request)
        {
            try
            {
                var embeddingResult = await faqManagementService.CreateFaqAsync(request.Question, request.Answer);
                if (embeddingResult.IsSuccess)
                {
                    return Results.Ok(new
                    {
                        Message = "FAQ created successfully.",
                    });
                }
                else
                {
                    return embeddingResult.ToProblemDetails();
                }
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }
    }
}
