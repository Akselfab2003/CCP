using System.Linq.Expressions;
using ChatService.Sdk.Models;

namespace ChatService.Sdk.Mappers
{
    internal static class FaqEntryProjections
    {
        public static readonly Expression<Func<FaqEntity, FaqModel>> FaqProjection = t => new FaqModel()
        {
            Id = t.Id.HasValue ? t.Id.Value : 0,
            Question = t.Question ?? string.Empty,
            Answer = t.Answer ?? string.Empty,
            Category = t.Category,
            CreatedAt = t.CreatedAt.HasValue ? t.CreatedAt.Value.UtcDateTime : DateTime.UtcNow,
            UpdatedAt = t.UpdatedAt.HasValue ? t.UpdatedAt.Value.UtcDateTime : DateTime.UtcNow,
        };

        public static FaqModel ToDto(this FaqEntity faqEntity)
            => FaqProjection.Compile().Invoke(faqEntity);

        public static IQueryable<FaqModel> ToDto(this IQueryable<FaqEntity> query)
            => query.Select(FaqProjection);

        public static List<FaqModel> ToDto(this List<FaqEntity> query)
            => [.. query.Select(FaqProjection.Compile())];

    }
}
