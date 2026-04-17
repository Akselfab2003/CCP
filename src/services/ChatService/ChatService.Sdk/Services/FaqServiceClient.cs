using CCP.Sdk.utils.Abstractions;
using CCP.Shared.ResultAbstraction;
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
    }
}
