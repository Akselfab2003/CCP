using CCP.Shared.ResultAbstraction;
using ChatService.Application.Models;
using ChatService.Application.Services.Faq;
using ChatService.Domain.Entities;
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
                    .RequireAuthorization("RequireManageFaq")
                    .Produces(StatusCodes.Status200OK)
                    .ProducesProblem(StatusCodes.Status400BadRequest)
                    .ProducesProblem(StatusCodes.Status500InternalServerError)
                    .ProducesProblem(StatusCodes.Status503ServiceUnavailable)
                    .ProducesProblem(StatusCodes.Status404NotFound)
                    .WithDisplayName("Create FAQ Embedding")
                    .WithDescription("Creates an embedding for a sample FAQ and returns it. In a real application, you would typically save the embedding to a database.");

            faqGroup.MapPatch("/Update", UpdateFaq)
                    .RequireAuthorization("RequireManageFaq")
                    .Produces(StatusCodes.Status200OK)
                    .ProducesProblem(StatusCodes.Status400BadRequest)
                    .ProducesProblem(StatusCodes.Status500InternalServerError)
                    .ProducesProblem(StatusCodes.Status503ServiceUnavailable)
                    .ProducesProblem(StatusCodes.Status404NotFound)
                    .WithDisplayName("Update FAQ Embedding")
                    .WithDescription("Updates an existing FAQ entry with a new embedding. This is a placeholder for demonstration purposes.");

            faqGroup.MapGet("/Search", SearchFaqs)
                    .RequireAuthorization("RequireManageFaq")
                    .Produces<List<FaqEntity>>(StatusCodes.Status200OK)
                    .ProducesProblem(StatusCodes.Status400BadRequest)
                    .ProducesProblem(StatusCodes.Status500InternalServerError)
                    .ProducesProblem(StatusCodes.Status503ServiceUnavailable)
                    .ProducesProblem(StatusCodes.Status404NotFound)
                    .WithDisplayName("Search FAQs")
                    .WithDescription("Searches for FAQs based on a query string. This is a placeholder for demonstration purposes.");

            faqGroup.MapGet("/GetAll", GetAllFaqs)
                    .RequireAuthorization("RequireManageFaq")
                    .Produces<List<FaqEntity>>(StatusCodes.Status200OK)
                    .ProducesProblem(StatusCodes.Status400BadRequest)
                    .ProducesProblem(StatusCodes.Status500InternalServerError)
                    .ProducesProblem(StatusCodes.Status503ServiceUnavailable)
                    .ProducesProblem(StatusCodes.Status404NotFound)
                    .WithDisplayName("Get All FAQs")
                    .WithDescription("Retrieves all FAQ entries. This is a placeholder for demonstration purposes.");

            faqGroup.MapDelete("/{faqId}", DeleteFaq)
                    .RequireAuthorization("RequireManageFaq")
                    .Produces(StatusCodes.Status200OK)
                    .ProducesProblem(StatusCodes.Status400BadRequest)
                    .ProducesProblem(StatusCodes.Status500InternalServerError)
                    .ProducesProblem(StatusCodes.Status503ServiceUnavailable)
                    .ProducesProblem(StatusCodes.Status404NotFound)
                    .WithDisplayName("Delete FAQ")
                    .WithDescription("Deletes an FAQ entry by its ID. This is a placeholder for demonstration purposes.");

            return endpoints;
        }

        private static async Task<IResult> DeleteFaq([FromServices] IFaqManagementService faqManagementService, [FromRoute] int faqId)
        {
            try
            {
                var result = await faqManagementService.DeleteFaqAsync(faqId);
                return result.IsSuccess ? Results.Ok() : result.ToProblemDetails();
            }
            catch (Exception)
            {
                return Results.Problem("An error occurred while deleting the FAQ.");
            }
        }

        private static async Task<IResult> GetAllFaqs([FromServices] IFaqManagementService faqManagementService)
        {
            try
            {
                var result = await faqManagementService.GetAllFaqsAsync();
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
            }
            catch (Exception)
            {
                return Results.Problem("An error occurred while retrieving FAQs.");
            }
        }

        private static async Task<IResult> SearchFaqs([FromServices] IFaqManagementService faqManagementService, [FromQuery] string query)
        {
            try
            {
                var result = await faqManagementService.SearchFaqAsync(query);
                return result.IsSuccess ? Results.Ok(result.Value) : result.ToProblemDetails();
            }
            catch (Exception)
            {
                return Results.Problem("An error occurred while searching FAQs.");
            }
        }

        private static async Task<IResult> UpdateFaq([FromServices] IFaqManagementService faqManagementService, [FromBody] UpdateFaqRequest request)
        {
            try
            {
                var result = await faqManagementService.UpdateFaqAsync(request.FaqId, request.Question, request.Answer, request.Category);
                return result.IsSuccess ? Results.Ok() : result.ToProblemDetails();
            }
            catch (Exception)
            {
                return Results.Problem("An error occurred while updating the FAQ.");
            }
        }


        private static async Task<IResult> CreateFaqEmbedding([FromServices] IFaqManagementService faqManagementService, [FromBody] CreateFaqRequest request)
        {
            try
            {
                var embeddingResult = await faqManagementService.CreateFaqAsync(request.Question, request.Answer);
                return embeddingResult.IsSuccess ? Results.Ok() : embeddingResult.ToProblemDetails();
            }
            catch (Exception ex)
            {
                return Results.Problem(ex.Message);
            }
        }
    }
}
