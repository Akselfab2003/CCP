using CCP.Shared.ResultAbstraction;
using ChatService.Application.Interfaces;
using ChatService.Domain.Interfaces;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChatService.Infrastructure.LLM.Chat
{
    public class ChatService : IChatService
    {
        private readonly IChatClient _chatClient;
        private readonly ILogger<ChatService> _logger;
        private readonly IEmbeddingService _embeddingService;
        private readonly IFaqRepository _faqRepository;
        public ChatService([FromKeyedServices("qwen")] IChatClient chatClient,
                           IEmbeddingService embeddingService,
                           ILogger<ChatService> logger,
                           IFaqRepository faqRepository)
        {
            _chatClient = chatClient;
            _embeddingService = embeddingService;
            _logger = logger;
            _faqRepository = faqRepository;
        }

        public async Task<Result<string>> GetChatResponseAsync(string userMessage)
        {
            try
            {
                // Step 1: Turn user message into embedding
                var userEmbedding = await _embeddingService.GenerateEmbeddingAsync(userMessage);
                var vector = new Pgvector.Vector(userEmbedding.Value.Vector);
                // Step 2: Retrieve relevant FAQs based on embedding similarity
                var relevantFaqs = await _faqRepository.SemanticSearch(vector);
                // Step 3: Construct system prompt with retrieved FAQs

                return Result.Success("This is a placeholder response. The actual implementation will construct a prompt with the retrieved FAQs and get a response from the chat client.");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting chat response.");
                return Result.Failure<string>(Error.Failure(code: "ChatServiceError", description: "An error occurred while processing your request. Please try again later."));
            }
        }

    }
}
