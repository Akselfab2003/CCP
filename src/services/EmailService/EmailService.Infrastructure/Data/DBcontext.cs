using System.Security.Cryptography;
using CCP.Shared.AuthContext;
using ChatApp.Encryption;
using EmailService.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EmailService.Infrastructure.Data
{
    public class DBcontext : DbContext
    {
        private readonly ICurrentUser _currentUser;
        private readonly IEncryptionService _encryptionService;
        public DBcontext(DbContextOptions<DBcontext> options, IEncryptionService encryptionService, ICurrentUser currentUser) : base(options)
        {
            _encryptionService = encryptionService;
            _currentUser = currentUser;
        }

        public DbSet<EmailSent> EmailSent { get; set; }
        public DbSet<EmailReceived> EmailReceived { get; set; }
        public DbSet<TenantEmailConfiguration> TenantEmailConfigurations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var encryptedConverter = new EncryptedStringConverter(_encryptionService);

            modelBuilder.Entity<TenantEmailConfiguration>(builder =>
            {
                builder.HasKey(t => t.Id);
                builder.Property(t => t.OrganizationId)
                       .IsRequired();
                builder.Property(t => t.InternalEmail)
                       .IsRequired();
                builder.Property(t => t.InternalEmailPassword)
                       .HasConversion(encryptedConverter)
                       .IsRequired();
                builder.Property(t => t.DefaultSenderEmail)
                       .IsRequired();

                builder.HasQueryFilter(t => t.OrganizationId == _currentUser.OrganizationId);
            });
        }
    }

    public class DBContextFactory : IDesignTimeDbContextFactory<DBcontext>
    {
        public DBcontext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<DBcontext>();
            optionsBuilder.UseNpgsql();
            var encryptionService = new AesEncryptionService(Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));
            var currentUser = new CurrentUser();
            return new DBcontext(optionsBuilder.Options, encryptionService, currentUser);
        }
    }
}
