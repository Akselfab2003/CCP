using CCP.Shared.AuthContext;
using CCP.Shared.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using TicketService.Api.IntegrationTests.Fixtures;
using TicketService.Domain.Entities;
using TicketService.Domain.Interfaces;
using TicketService.Sdk.Services.Assignment;

namespace TicketService.Api.IntegrationTests.Tests
{
    [Collection("TicketService")]
    public class AssignmentApiServiceTests
    {
        private readonly TicketServiceFixture _fixture;
        public AssignmentApiServiceTests(TicketServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task AssignTicketToUserAsync_ShouldAssignTicketSuccessfully()
        {
            // Arrange
            using var SDK = _fixture.SDK.CreateAsyncScope();
            using var DB = _fixture.DB.CreateAsyncScope();
            Guid userId = Guid.NewGuid(); // Replace with a valid user ID from your test setup
            Guid OrgId = Guid.NewGuid();
            ICurrentUser currentUser = DB.ServiceProvider.GetRequiredService<ICurrentUser>();
            IAssignmentRepository assignmentRepo = DB.ServiceProvider.GetRequiredService<IAssignmentRepository>();
            IAssignmentService assignmentService = SDK.ServiceProvider.GetRequiredService<Sdk.Services.Assignment.IAssignmentService>();
            currentUser.SetCurrentUser(userId);
            currentUser.SetOrganizationId(OrgId);

            var ticket = await SetupTestTicket(OrgId, userId);

            // Act
            var assignmentResult = await assignmentService.AssignTicketToUserAsync(ticketId: ticket.Id,
                                                              userId: userId,
                                                              cancellationToken: TestContext.Current.CancellationToken);
            // Assert
            Assert.NotNull(assignmentResult);
            Assert.True(assignmentResult.IsSuccess, $"Expected assignment to succeed, but it failed with error: {assignmentResult.Error?.Description}");

            var assignmentFromDbResult = await assignmentRepo.GetAssignmentByTicketIdAsync(ticket.Id);
            Assert.NotNull(assignmentFromDbResult);
            Assert.True(assignmentFromDbResult.IsSuccess, $"Expected to retrieve assignment from DB, but failed with error: {assignmentFromDbResult.Error?.Description}");
            Assert.Equal(userId, assignmentFromDbResult.Value.UserId);
        }


        [Fact]
        public async Task AssignTicketToUserAsync_ShouldFailForNonExistentTicket()
        {
            // Arrange
            using var SDK = _fixture.SDK.CreateAsyncScope();
            Guid userId = Guid.NewGuid(); // Replace with a valid user ID from your test setup
            IAssignmentService assignmentService = SDK.ServiceProvider.GetRequiredService<Sdk.Services.Assignment.IAssignmentService>();
            int nonExistentTicketId = -1; // Assuming negative IDs do not exist
            // Act
            var assignmentResult = await assignmentService.AssignTicketToUserAsync(ticketId: nonExistentTicketId,
                                                              userId: userId,
                                                              cancellationToken: TestContext.Current.CancellationToken);
            // Assert
            Assert.NotNull(assignmentResult);
            Assert.False(assignmentResult.IsSuccess, "Expected assignment to fail for non-existent ticket, but it succeeded.");
        }


        [Fact]
        public async Task AssignTicketToUserAsync_ShouldOverideExistingAssignment()
        {
            // Arrange
            using var SDK = _fixture.SDK.CreateAsyncScope();
            using var DB = _fixture.DB.CreateAsyncScope();
            Guid userId1 = Guid.NewGuid(); // Replace with a valid user ID from your test setup
            Guid userId2 = Guid.NewGuid(); // Replace with another valid user ID from your test setup
            Guid OrgId = Guid.NewGuid();
            ICurrentUser currentUser = DB.ServiceProvider.GetRequiredService<ICurrentUser>();
            IAssignmentRepository assignmentRepo = DB.ServiceProvider.GetRequiredService<IAssignmentRepository>();
            IAssignmentService assignmentService = SDK.ServiceProvider.GetRequiredService<Sdk.Services.Assignment.IAssignmentService>();
            currentUser.SetCurrentUser(userId1);
            currentUser.SetOrganizationId(OrgId);
            var ticket = await SetupTestTicket(OrgId, userId1);
            // Act - First assignment
            var firstAssignmentResult = await assignmentService.AssignTicketToUserAsync(ticketId: ticket.Id,
                                                              userId: userId1,
                                                              cancellationToken: TestContext.Current.CancellationToken);
            Assert.NotNull(firstAssignmentResult);
            Assert.True(firstAssignmentResult.IsSuccess, $"Expected first assignment to succeed, but it failed with error: {firstAssignmentResult.Error?.Description}");
            // Act - Second assignment to a different user
            var secondAssignmentResult = await assignmentService.AssignTicketToUserAsync(ticketId: ticket.Id,
                                                              userId: userId2,
                                                              cancellationToken: TestContext.Current.CancellationToken);
            Assert.NotNull(secondAssignmentResult);
            Assert.True(secondAssignmentResult.IsSuccess, $"Expected second assignment to succeed, but it failed with error: {secondAssignmentResult.Error?.Description}");
            // Assert - Verify the latest assignment in the database
            var assignmentFromDbResult = await assignmentRepo.GetAssignmentByTicketIdAsync(ticket.Id);
            Assert.NotNull(assignmentFromDbResult);
            Assert.True(assignmentFromDbResult.IsSuccess, $"Expected to retrieve assignment from DB, but failed with error: {assignmentFromDbResult.Error?.Description}");
            Assert.Equal(userId2, assignmentFromDbResult.Value.UserId);
        }


        private async Task<Ticket> SetupTestTicket(Guid org, Guid userId)
        {
            using var DB = _fixture.DB.CreateAsyncScope();
            ITicketRepositoryCommands ticketRepo = DB.ServiceProvider.GetRequiredService<ITicketRepositoryCommands>();
            ICurrentUser currentUser = DB.ServiceProvider.GetRequiredService<ICurrentUser>();
            currentUser.SetOrganizationId(org);
            currentUser.SetCurrentUser(userId);

            var ticket = await ticketRepo.AddAsync(new Domain.Entities.Ticket()
            {
                Title = "Test Ticket",
                OrganizationId = org,
                CreatedAt = DateTime.UtcNow,
                Status = TicketStatus.Open,
                CustomerId = Guid.NewGuid()
            });

            await ticketRepo.SaveChangesAsync();

            return ticket.Value;
        }
    }
}
