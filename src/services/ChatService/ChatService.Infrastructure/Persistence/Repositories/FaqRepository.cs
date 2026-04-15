using CCP.Shared.ResultAbstraction;
using ChatService.Domain.Entities;
using ChatService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace ChatService.Infrastructure.Persistence.Repositories
{
    public class FaqRepository : IFaqRepository
    {
        private readonly ILogger<FaqRepository> _logger;
        private readonly ChatDbContext _context;

        public FaqRepository(ILogger<FaqRepository> logger, ChatDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<Result> AddAsync(FaqEntity faq, CancellationToken ct = default)
        {
            try
            {
                await _context.FaqEntries.AddAsync(faq, ct);
                await _context.SaveChangesAsync(ct);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not add FAQ entry with question: {Question}", faq.Question);
                return Result.Failure(Error.Failure("FaqAddFailed", "An error occurred while adding the FAQ entry."));
            }
        }

        public async Task<Result<List<FaqEntity>>> SemanticSearch(Vector searchVector, int topK = 5, CancellationToken ct = default)
        {
            try
            {
                var faqs = await _context.FaqEntries.Where(f => f.Embedding != null)
                                                    .OrderBy(f => f.Embedding!.L2Distance(searchVector))
                                                    .Take(topK)
                                                    .ToListAsync(cancellationToken: ct);
                return Result.Success(faqs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not perform semantic search with vector");
                return Result.Failure<List<FaqEntity>>(Error.Failure("FaqSearchFailed", "An error occurred while performing semantic search for FAQ entries."));
            }
        }


        public async Task<Result<List<FaqEntity>>> GetAllAsync()
        {
            try
            {
                var faqs = await _context.FaqEntries.ToListAsync();
                return Result.Success(faqs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not retrieve FAQ entries");
                return Result.Failure<List<FaqEntity>>(Error.Failure("FaqRetrievalFailed", "An error occurred while retrieving FAQ entries."));
            }
        }
    }
}
