using ChatService.Domain.Entities;
using ChatService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace ChatService.Repositories;

public interface IFaqRepository
{
    Task<List<FaqEntity>> SearchSimilarAsync(
        float[] queryEmbedding, Guid orgId,
        int topK, double threshold, CancellationToken ct = default);

    Task UpsertAsync(FaqEntity entry, float[] embedding, CancellationToken ct = default);
}

public class FaqRepository : IFaqRepository
{
    private readonly ChatDbContext _db;
    private readonly ILogger<FaqRepository> _logger;

    public FaqRepository(ChatDbContext db, ILogger<FaqRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<FaqEntity>> SearchSimilarAsync(
        float[] queryEmbedding, Guid orgId,
        int topK, double threshold, CancellationToken ct = default)
    {
        // Returner tom liste hvis embedding fejlede
        if (queryEmbedding == null || queryEmbedding.Length == 0)
        {
            _logger.LogWarning("Tomt embedding — springer FAQ søgning over.");
            return [];
        }

        var vector = new Vector(queryEmbedding);

        return await _db.FaqEntries
            .Where(f => f.OrgId == orgId || f.OrgId == null)
            .OrderBy(f => f.Embedding.CosineDistance(vector))
            .Where(f => 1 - f.Embedding.CosineDistance(vector) >= threshold)
            .Take(topK)
            .Select(f => new FaqEntity
            {
                Id = f.Id,
                Question = f.Question,
                Answer = f.Answer,
                Category = f.Category,
                OrgId = f.OrgId
            })
            .ToListAsync(ct);
    }

    public async Task UpsertAsync(FaqEntity entry, float[] embedding, CancellationToken ct = default)
    {
        var existing = await _db.FaqEntries.FindAsync([entry.Id], ct);

        if (existing is null)
        {
            entry.Embedding = embedding;
            await _db.FaqEntries.AddAsync(entry, ct);
        }
        else
        {
            existing.Question = entry.Question;
            existing.Answer = entry.Answer;
            existing.Category = entry.Category;
            existing.Embedding = embedding;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
    }
}
