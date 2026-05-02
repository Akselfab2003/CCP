using CCP.Shared.AuthContext;
using ChatApp.Encryption;
using CustomerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.Api.DB
{
    // Database context for customer data persistence
    public class CustomerDBContext : DbContext
    {
        private readonly IEncryptionService _encryptionService;
        private readonly ICurrentUser _currentUser;

        public CustomerDBContext(DbContextOptions<CustomerDBContext> options, IEncryptionService encryptionService, ICurrentUser currentUser)
            : base(options)
        {
            _encryptionService = encryptionService;
            _currentUser = currentUser;
        }

        // Customer table
        public DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Opret converter instance
            var encryptedConverter = new EncryptedStringConverter(_encryptionService);

            // Konfigurer Customer entity
            modelBuilder.Entity<Customer>(entity =>
            {
                // Email skal encryptes i databasen
                entity.Property(e => e.Email)
                    .HasConversion(encryptedConverter)
                    .HasMaxLength(500); // Encrypted data er længere end plaintext

                // Name skal encryptes i databasen
                entity.Property(e => e.Name)
                    .HasConversion(encryptedConverter)
                    .HasMaxLength(500);
            });

            modelBuilder.Entity<Customer>().HasQueryFilter(c => c.OrganizationId == _currentUser.OrganizationId);
        }
    }
}
