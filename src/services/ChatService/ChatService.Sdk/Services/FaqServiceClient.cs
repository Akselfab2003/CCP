using CCP.Sdk.utils.Abstractions;
using CCP.Shared.ResultAbstraction;
using ChatService.Sdk.Mappers;
using ChatService.Sdk.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Kiota.Abstractions;

namespace ChatService.Sdk.Services
{
    internal class FaqServiceClient : IFaqService
    {
        private readonly IKiotaApiClient<ChatServiceClient> _client;
        private readonly ILogger<FaqServiceClient> _logger;

        public FaqServiceClient(IKiotaApiClient<ChatServiceClient> client, ILogger<FaqServiceClient> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<Result> CreateNewFaqEntry(string question, string answer)
        {
            try
            {
                var faqEntry = new CreateFaqRequest()
                {
                    Question = question,
                    Answer = answer
                };
                await _client.Client.Faqs.PostAsync(faqEntry);

                return Result.Success();
            }
            catch (ApiException apiex)
            {
                return apiex.ResponseStatusCode switch
                {
                    400 => Result.Failure(Error.Validation(code: "BadRequest", description: "Invalid FAQ entry data.")),
                    401 => Result.Failure(Error.Failure(code: "Unauthorized", description: "Unauthorized access to the API.")),
                    404 => Result.Failure(Error.NotFound(code: "NotFound", description: "API endpoint not found.")),
                    500 => Result.Failure(Error.Failure(code: "ServerError", description: "Internal server error occurred.")),
                    _ => Result.Failure(Error.Failure(code: "ApiError", description: $"API error occurred: {apiex.Message}"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new FAQ entry");
                return Result.Failure(Error.Failure(code: "Exception", description: $"An unexpected error occurred: {ex.Message}"));
            }
        }

        public async Task<Result<List<FaqModel>>> SearchFaqEntries(string query)
        {
            try
            {
                var faqEntries = await _client.Client.Faqs.Search.GetAsync(r =>
                {
                    r.QueryParameters.Query = query;
                });

                if (faqEntries == null) return Result.Failure<List<FaqModel>>(Error.NotFound(code: "NotFound", description: "No FAQ entries found for the search query."));
                var faqModels = faqEntries.Select(f => f.ToDto()).ToList();
                return Result.Success(faqModels);
            }
            catch (ApiException ex)
            {
                return ex.ResponseStatusCode switch
                {
                    400 => Result.Failure<List<FaqModel>>(Error.Validation(code: "BadRequest", description: "Invalid search query for FAQ entries.")),
                    401 => Result.Failure<List<FaqModel>>(Error.Failure(code: "Unauthorized", description: "Unauthorized access to the API.")),
                    404 => Result.Failure<List<FaqModel>>(Error.NotFound(code: "NotFound", description: "API endpoint not found.")),
                    500 => Result.Failure<List<FaqModel>>(Error.Failure(code: "ServerError", description: "Internal server error occurred.")),
                    _ => Result.Failure<List<FaqModel>>(Error.Failure(code: "ApiError", description: $"API error occurred: {ex.Message}"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching FAQ entries with query {Query}", query);
                return Result.Failure<List<FaqModel>>(Error.Failure(code: "Exception", description: $"An unexpected error occurred: {ex.Message}"));
            }
        }

        public async Task<Result<List<FaqModel>>> GetAllFaqEntries()
        {
            try
            {
                var faqEntries = await _client.Client.Faqs.GetAll.GetAsync();
                if (faqEntries == null) return Result.Failure<List<FaqModel>>(Error.NotFound(code: "NotFound", description: "No FAQ entries found."));
                var faqModels = faqEntries.Select(f => f.ToDto()).ToList();
                return Result.Success(faqModels);
            }
            catch (ApiException ex)
            {
                return ex.ResponseStatusCode switch
                {
                    400 => Result.Failure<List<FaqModel>>(Error.Validation(code: "BadRequest", description: "Invalid request for FAQ entries.")),
                    401 => Result.Failure<List<FaqModel>>(Error.Failure(code: "Unauthorized", description: "Unauthorized access to the API.")),
                    404 => Result.Failure<List<FaqModel>>(Error.NotFound(code: "NotFound", description: "API endpoint not found.")),
                    500 => Result.Failure<List<FaqModel>>(Error.Failure(code: "ServerError", description: "Internal server error occurred.")),
                    _ => Result.Failure<List<FaqModel>>(Error.Failure(code: "ApiError", description: $"API error occurred: {ex.Message}"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving FAQ entries");
                return Result.Failure<List<FaqModel>>(Error.Failure(code: "Exception", description: $"An unexpected error occurred: {ex.Message}"));
            }
        }

        public async Task<Result> UpdateFaq(int id, string question, string answer, string category)
        {
            try
            {
                var faqEntry = new UpdateFaqRequest()
                {
                    FaqId = id,
                    Question = question,
                    Answer = answer,
                    Category = category
                };
                await _client.Client.Faqs.Update.PatchAsync(faqEntry);
                return Result.Success();
            }
            catch (ApiException ex)
            {
                return ex.ResponseStatusCode switch
                {
                    400 => Result.Failure(Error.Validation(code: "BadRequest", description: "Invalid FAQ entry data or ID.")),
                    401 => Result.Failure(Error.Failure(code: "Unauthorized", description: "Unauthorized access to the API.")),
                    404 => Result.Failure(Error.NotFound(code: "NotFound", description: "FAQ entry not found.")),
                    500 => Result.Failure(Error.Failure(code: "ServerError", description: "Internal server error occurred.")),
                    _ => Result.Failure(Error.Failure(code: "ApiError", description: $"API error occurred: {ex.Message}"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating FAQ entry with ID {FaqId}", id);
                return Result.Failure(Error.Failure(code: "Exception", description: $"An unexpected error occurred: {ex.Message}"));
            }
        }

        public async Task<Result> DeleteFaq(int id)
        {
            try
            {
                await _client.Client.Faqs[id].DeleteAsync();
                return Result.Success();
            }
            catch (ApiException ex)
            {
                return ex.ResponseStatusCode switch
                {
                    400 => Result.Failure(Error.Validation(code: "BadRequest", description: "Invalid FAQ entry ID.")),
                    401 => Result.Failure(Error.Failure(code: "Unauthorized", description: "Unauthorized access to the API.")),
                    404 => Result.Failure(Error.NotFound(code: "NotFound", description: "FAQ entry not found.")),
                    500 => Result.Failure(Error.Failure(code: "ServerError", description: "Internal server error occurred.")),
                    _ => Result.Failure(Error.Failure(code: "ApiError", description: $"API error occurred: {ex.Message}"))
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting FAQ entry with ID {FaqId}", id);
                return Result.Failure(Error.Failure(code: "Exception", description: $"An unexpected error occurred: {ex.Message}"));
            }
        }
    }
}
