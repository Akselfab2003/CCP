using ChatService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Pgvector;

namespace ChatService.Data;

public class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }

    public DbSet<FaqEntry> FaqEntries => Set<FaqEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<FaqEntry>(entity =>
        {
            entity.ToTable("faq_entries");
            entity.HasKey(e => e.Id);

            var converter = new ValueConverter<float[], Vector>(
                v => new Vector(v),
                v => v.ToArray());

            // Value comparer til float[]
            var comparer = new ValueComparer<float[]>(
                (a, b) => a != null && b != null && a.SequenceEqual(b),
                v => v.Aggregate(0, (a, e) => HashCode.Combine(a, e.GetHashCode())),
                v => v.ToArray());

            entity.Property(e => e.Embedding)
                  .HasColumnType("vector(768)")
                  .HasConversion(converter)
                  .Metadata.SetValueComparer(comparer);

            entity.HasIndex(e => e.Embedding)
                  .HasMethod("ivfflat")
                  .HasOperators("vector_cosine_ops")
                  .HasStorageParameter("lists", 100);
        });
    }
}
