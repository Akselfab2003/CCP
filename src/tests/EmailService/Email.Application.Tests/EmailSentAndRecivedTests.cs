using EmailService.Domain.Models;
using EmailService.Infrastructure.Data;
using EmailService.Infrastructure.EmailInfrastructure;
using Microsoft.EntityFrameworkCore;

namespace Email.Application.Tests
{
    internal static class DbFactory
    {
        public static DBcontext Create(string dbName)
        {
            var options = new DbContextOptionsBuilder<DBcontext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new DBcontext(options);
        }
    }

    public class EmailReceivedRepoTests
    {
        [Fact]
        public async Task CreateAsync_WithValidEmail_ReturnsSuccess()
        {
            await using var db = DbFactory.Create(nameof(CreateAsync_WithValidEmail_ReturnsSuccess));
            var repo = new EmailReceivedRepo(db);

            var result = await repo.CreateAsync(new EmailReceived
            {
                OrganizationId = Guid.NewGuid(),
                Subject = "Test",
                Body = "Body",
                SenderAddress = "from@test.com",
                RecipientAddress = "to@test.com",
                ReceivedAt = DateTime.UtcNow
            });

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task CreateAsync_WithNullEmail_ReturnsFailure()
        {
            await using var db = DbFactory.Create(nameof(CreateAsync_WithNullEmail_ReturnsFailure));
            var repo = new EmailReceivedRepo(db);

            var result = await repo.CreateAsync(null!);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentId_ReturnsNotFoundFailure()
        {
            await using var db = DbFactory.Create(nameof(DeleteAsync_WithNonExistentId_ReturnsNotFoundFailure));
            var repo = new EmailReceivedRepo(db);

            var result = await repo.DeleteAsync(9999);

            Assert.False(result.IsSuccess);
            Assert.Contains("9999", result.Error.Description);
        }
    }

    public class EmailSentRepoTests
    {
        [Fact]
        public async Task CreateAsync_WithValidEmail_ReturnsSuccess()
        {
            await using var db = DbFactory.Create(nameof(CreateAsync_WithValidEmail_ReturnsSuccess));
            var repo = new EmailSentRepo(db);

            var result = await repo.CreateAsync(new EmailSent
            {
                OrganizationId = Guid.NewGuid(),
                Subject = "Test",
                Body = "Body",
                SenderAddress = "from@test.com",
                RecipientAddress = "to@test.com",
                SentAt = DateTime.UtcNow
            });

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task CreateAsync_WithNullEmail_ReturnsFailure()
        {
            await using var db = DbFactory.Create(nameof(CreateAsync_WithNullEmail_ReturnsFailure));
            var repo = new EmailSentRepo(db);

            var result = await repo.CreateAsync(null!);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentId_ReturnsNotFoundFailure()
        {
            await using var db = DbFactory.Create(nameof(DeleteAsync_WithNonExistentId_ReturnsNotFoundFailure));
            var repo = new EmailSentRepo(db);

            var result = await repo.DeleteAsync(9999);

            Assert.False(result.IsSuccess);
            Assert.Contains("9999", result.Error.Description);
        }
    }
}
