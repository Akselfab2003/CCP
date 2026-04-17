using CCP.Shared.AuthContext;
using CCP.Shared.ResultAbstraction;
using ChatService.Application.Interfaces;
using ChatService.Domain.Entities;
using ChatService.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ChatService.Application.Services.Faq
{
    public class FaqManagementService : IFaqManagementService
    {
        private readonly ILogger<FaqManagementService> _logger;
        private readonly IEmbeddingService _embeddingService;
        private readonly IFaqRepository _faqRepository;
        private readonly ICurrentUser _currentUser;

        public FaqManagementService(ILogger<FaqManagementService> logger, IEmbeddingService embeddingService, IFaqRepository faqRepository, ICurrentUser currentUser)
        {
            _logger = logger;
            _embeddingService = embeddingService;
            _faqRepository = faqRepository;
            _currentUser = currentUser;
        }

        public async Task<Result> CreateFaqAsync(string question, string answer)
        {
            try
            {
                var embeddingResult = await _embeddingService.GenerateEmbeddingAsync(question);
                if (embeddingResult.IsFailure) return embeddingResult;


                var embedding = embeddingResult.Value.Vector;

                var faq = new FaqEntity()
                {
                    Id = 0, // Id will be set by the database
                    OrgId = _currentUser.OrganizationId,
                    Question = question,
                    Answer = answer,
                    Embedding = new Pgvector.Vector(embedding),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Category = null
                };

                var savedFaq = await _faqRepository.AddAsync(faq);

                return savedFaq;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating FAQ.");
                return Result.Failure(Error.Failure("FaqCreationError", "An error occurred while creating the FAQ."));
            }
        }

        public async Task<Result<List<FaqEntity>>> GetRelevantFaqAsync(string question)
        {
            try
            {
                var embeddingResult = await _embeddingService.GenerateEmbeddingAsync(question);
                if (embeddingResult.IsFailure) return Result.Failure<List<FaqEntity>>(embeddingResult.Error);

                var embedding = new Pgvector.Vector(embeddingResult.Value.Vector);

                var SimilarFaq = await _faqRepository.SemanticSearch(embedding);

                if (SimilarFaq.IsFailure) return Result.Failure<List<FaqEntity>>(SimilarFaq.Error);

                return SimilarFaq;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving relevant FAQs.");
                return Result.Failure<List<FaqEntity>>(Error.Failure("FaqRetrievalError", "An error occurred while retrieving relevant FAQs."));
            }
        }
    }
}
