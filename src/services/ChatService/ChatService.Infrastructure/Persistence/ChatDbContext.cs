using CCP.Shared.AuthContext;
using ChatService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ChatService.Infrastructure.Persistence;

public class ChatDbContext : DbContext
{
    private readonly ICurrentUser _currentUser;
    private bool _IsDesignTime;
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        _IsDesignTime = !optionsBuilder.IsConfigured;
    }

    public ChatDbContext(DbContextOptions<ChatDbContext> options, ICurrentUser currentUser) : base(options)
    {
        _currentUser = currentUser ?? throw new InvalidOperationException("CurrentUser service is not available.");
    }

    public DbSet<FaqEntity> FaqEntries => Set<FaqEntity>();
    public DbSet<SessionEntity> Sessions => Set<SessionEntity>();
    public DbSet<ConversationEntity> Conversations => Set<ConversationEntity>();
    public DbSet<MessageEntity> Messages => Set<MessageEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");
        if (_IsDesignTime)
        {
            modelBuilder.ApplyConfiguration(new Configurations.SessionEntityConfiguration());
            modelBuilder.ApplyConfiguration(new Configurations.FaqEntityConfiguration());
            modelBuilder.ApplyConfiguration(new Configurations.ConversationEntityConfiguration());
            modelBuilder.ApplyConfiguration(new Configurations.MessageEntityConfiguration());
            return;
        }
        else
        {
            modelBuilder.ApplyConfiguration(new Configurations.SessionEntityConfiguration());
            modelBuilder.ApplyConfiguration(new Configurations.FaqEntityConfiguration());
            modelBuilder.ApplyConfiguration(new Configurations.ConversationEntityConfiguration());
            modelBuilder.ApplyConfiguration(new Configurations.MessageEntityConfiguration());

            modelBuilder.Entity<SessionEntity>()
                .HasQueryFilter(s => s.OrganizationId == _currentUser.OrganizationId);

            modelBuilder.Entity<ConversationEntity>()
                .HasQueryFilter(c => c.OrgId == _currentUser.OrganizationId);
            modelBuilder.Entity<MessageEntity>()
               .HasQueryFilter(m => m.OrgId == _currentUser.OrganizationId);
        }
    }
}

public class ChatDbContextFactory : IDesignTimeDbContextFactory<ChatDbContext>
{
    public ChatDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ChatDbContext>();

        optionsBuilder.UseNpgsql(a =>
        {
            a.UseVector();
        });
        return new ChatDbContext(optionsBuilder.Options, new CurrentUser());
    }
}
