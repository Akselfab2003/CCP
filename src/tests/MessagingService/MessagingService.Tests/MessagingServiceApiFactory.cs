using System.Security.Cryptography;
using Aspire.Hosting;
using Aspire.Hosting.Testing;
using MessagingService.Domain.Entities;
using MessagingService.Domain.Interfaces;
using MessagingService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using TestProject = Projects.CCP_AppHost;
namespace ChatApp.MessagingService.Tests;

public class TestApiMessagingDbContext : MessagingDbContext
{
    public TestApiMessagingDbContext(DbContextOptions<MessagingDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(m => m.Id);

            entity.Property(m => m.TicketId).IsRequired();
            entity.Property(m => m.OrganizationId).IsRequired();
            entity.Property(m => m.UserId).IsRequired(false);

            entity.Property(m => m.Content)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(m => m.CreatedAtUtc).IsRequired();
            entity.Property(m => m.UpdatedAtUtc);
            entity.Property(m => m.DeletedAtUtc);

            entity.Property(m => m.IsEdited).IsRequired();
            entity.Property(m => m.IsDeleted).IsRequired();

            entity.Ignore(m => m.Embedding);
        });
    }
}

public class AllowAllIntegrationTestMessageAccessValidator : IMessageAccessValidator
{
    public Task<bool> CanSendMessageAsync(
        int ticketId,
        Guid organizationId,
        Guid? userId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }
}

public class MessagingServiceApiFactory : WebApplicationFactory<TestProgramNameSpace.Program>
{
    private IDistributedApplicationTestingBuilder Apphost { get; set; } = null!;
    private DistributedApplication App { get; set; } = null!;
    private IDistributedApplicationTestingBuilder RemoveNotNeededResourcesForTesting(IDistributedApplicationTestingBuilder AppHost)
    {
        var ResourcesToKeepName = new List<string>()
            {
                "MessagingDatabase",
                "postgres",
            };

        var resources = AppHost.Resources.Where(r => !ResourcesToKeepName.Contains(r.Name))
                                         .ToList();
        foreach (var resource in resources)
        {
            AppHost.Resources.Remove(resource);
        }

        return AppHost;
    }
    private string GenerateEncryptionKey()
    {
        var keyBase = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        var decoded = Convert.FromBase64String(keyBase);

        if (decoded.Length != 32)
        {
            throw new InvalidOperationException("Generated encryption key is not 256 bits (32 bytes) long.");
        }

        return keyBase;
    }
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Apphost = DistributedApplicationTestingBuilder.CreateAsync<TestProject>([
            "DcpPublisher:RandomizePorts=false",
    $"KeycloakAdminApiClientSecret={Guid.NewGuid()}",
    "ENVIORMENT=Tests",
    $"SERVICE_ACCOUNT_SECRET={Guid.NewGuid()}",
    "emailWorkerServiceUsername=test@test.test",
     $"Encryption_Key={GenerateEncryptionKey()}",
    "emailWorkerServicePassword=test",
                    "ROUNDCUBE_DEFAULT_USER_EMAIL=test@test.test",
                "ROUNDCUBE_DEFAULT_USER_PASSWORD=test",
        // REALM_IMPORT_PATH intentionally omitted — Keycloak removed during tests
        ], CancellationToken.None).Result;

        Apphost = RemoveNotNeededResourcesForTesting(Apphost);


        App = Apphost.BuildAsync(CancellationToken.None).WaitAsync(TimeSpan.FromMinutes(3), CancellationToken.None).Result;

        App.StartAsync(CancellationToken.None).WaitAsync(TimeSpan.FromMinutes(3), CancellationToken.None).Wait();
        App.ResourceNotifications.WaitForResourceHealthyAsync("MessagingDatabase", CancellationToken.None).WaitAsync(TimeSpan.FromMinutes(3), CancellationToken.None).Wait();

        var DbConnectionString = App.GetConnectionStringAsync("MessagingDatabase").Result;

        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<MessagingDbContext>));
            services.RemoveAll(typeof(MessagingDbContext));
            services.RemoveAll(typeof(TestApiMessagingDbContext));
            services.RemoveAll(typeof(IMessageAccessValidator));

            var databaseName = Guid.NewGuid().ToString();

            services.AddScoped(_ =>
                new DbContextOptionsBuilder<MessagingDbContext>()
                    .UseNpgsql(DbConnectionString, options =>
                    {
                        options.UseVector();
                    })
                    .Options);

            services.AddScoped<TestApiMessagingDbContext>();

            services.AddScoped<MessagingDbContext>(sp =>
                sp.GetRequiredService<TestApiMessagingDbContext>());

            services.AddScoped<IMessageAccessValidator, AllowAllIntegrationTestMessageAccessValidator>();
        });


    }
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = builder.Build();

        // Use the migration helper, passing a WebApplication if available
        // Apply migrations after DI container is configured
        using (var scope = host.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<MessagingDbContext>();
            Assert.True(dbContext.Database.EnsureCreated());
            Assert.True(dbContext.Database.CanConnect());
            dbContext.Database.Migrate();
        }

        host.Start();
        return host;
    }
}
