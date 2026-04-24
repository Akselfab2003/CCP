using CCP.Shared.AuthContext;
using CCP.Shared.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using TicketService.Domain.Entities;
using TicketService.Domain.Interfaces;
using TicketService.Infrastructure.Tests.Fixtures;

namespace TicketService.Infrastructure.Tests.Tests
{
    [Collection("TicketServiceInfrastructure")]
    public class TicketServiceTests
    {
        private readonly TicketServiceFixture _fixture;

        public TicketServiceTests(TicketServiceFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task AddAsync_ShouldAddTicketSuccessfully()
        {
            // Arrange
            using var DbScope = _fixture.DB.CreateAsyncScope();
            ICurrentUser currentUser = DbScope.ServiceProvider.GetRequiredService<ICurrentUser>();
            currentUser.SetOrganizationId(Guid.NewGuid());

            // Get the repository from the service provider
            ITicketRepositoryCommands ticketRepositoryCommands = DbScope.ServiceProvider.GetRequiredService<ITicketRepositoryCommands>();
            var ticket = new Ticket
            {
                Title = "Test Ticket",
                CustomerId = Guid.NewGuid(),
                Status = TicketStatus.Open,
                OrganizationId = currentUser.OrganizationId,
                CreatedAt = DateTime.UtcNow,
            };

            // Act
            var result = await ticketRepositoryCommands.AddAsync(ticket);
            await ticketRepositoryCommands.SaveChangesAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(ticket.Title, result.Value.Title);
        }

        [Fact]
        public async Task GetTicketsBasedOnParameters_ShouldFilterByStatus()
        {
            // Arrange
            using var DbScope = _fixture.DB.CreateAsyncScope();
            ICurrentUser currentUser = DbScope.ServiceProvider.GetRequiredService<ICurrentUser>();
            currentUser.SetOrganizationId(Guid.NewGuid());

            // Get the repository from the service provider
            ITicketRepositoryCommands ticketRepositoryCommands = DbScope.ServiceProvider.GetRequiredService<ITicketRepositoryCommands>();

            var openTicket = new Ticket
            {
                Title = "open",
                CustomerId = Guid.NewGuid(),
                Status = TicketStatus.Open,
                OrganizationId = currentUser.OrganizationId,
                CreatedAt = DateTime.UtcNow,
            };
            var closedTicket = new Ticket
            {
                Title = "closed",
                CustomerId = Guid.NewGuid(),
                Status = TicketStatus.Closed,
                OrganizationId = currentUser.OrganizationId,
                CreatedAt = DateTime.UtcNow,
            };
            await ticketRepositoryCommands.AddAsync(openTicket);
            await ticketRepositoryCommands.AddAsync(closedTicket);
            await ticketRepositoryCommands.SaveChangesAsync();

            // Act
            var result = await ticketRepositoryCommands.GetTicketsBasedOnParameters(status: TicketStatus.Open);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value);
            Assert.Equal("open", result.Value[0].Title);
        }

        [Fact]
        public async Task GetTicketsBasedOnParameters_ShouldFilterByCustomerId()
        {
            // Arrange
            using var DbScope = _fixture.DB.CreateAsyncScope();
            ICurrentUser currentUser = DbScope.ServiceProvider.GetRequiredService<ICurrentUser>();
            currentUser.SetOrganizationId(Guid.NewGuid());

            // Get the repository from the service provider
            ITicketRepositoryCommands ticketRepositoryCommands = DbScope.ServiceProvider.GetRequiredService<ITicketRepositoryCommands>();
            var customerId = Guid.NewGuid();

            var ticket1 = new Ticket
            {
                Title = "Ticket 1",
                CustomerId = customerId,
                Status = TicketStatus.Open,
                OrganizationId = currentUser.OrganizationId,
                CreatedAt = DateTime.UtcNow,
            };
            var ticket2 = new Ticket
            {
                Title = "Ticket 2",
                CustomerId = Guid.NewGuid(),
                Status = TicketStatus.Open,
                OrganizationId = currentUser.OrganizationId,
                CreatedAt = DateTime.UtcNow,
            };
            await ticketRepositoryCommands.AddAsync(ticket1);
            await ticketRepositoryCommands.AddAsync(ticket2);
            await ticketRepositoryCommands.SaveChangesAsync();

            // Act
            var result = await ticketRepositoryCommands.GetTicketsBasedOnParameters(CustomerId: customerId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value);
            Assert.Equal("Ticket 1", result.Value[0].Title);
        }

        [Fact]
        public async Task GetTicketsBasedOnParameters_WhenNotExists_ShouldReturnEmptyList()
        {
            // Arrange
            using var DbScope = _fixture.DB.CreateAsyncScope();
            ICurrentUser currentUser = DbScope.ServiceProvider.GetRequiredService<ICurrentUser>();
            currentUser.SetOrganizationId(Guid.NewGuid());


            // Get the repository from the service provider
            ITicketRepositoryCommands ticketRepositoryCommands = DbScope.ServiceProvider.GetRequiredService<ITicketRepositoryCommands>();

            // Act
            var result = await ticketRepositoryCommands.GetTicketsBasedOnParameters(CustomerId: Guid.NewGuid());

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Empty(result.Value);
        }

        [Fact]
        public async Task GetTicket_ShouldReturnTicket_WhenExists()
        {
            // Arrange
            using var DbScope = _fixture.DB.CreateAsyncScope();
            ICurrentUser currentUser = DbScope.ServiceProvider.GetRequiredService<ICurrentUser>();
            currentUser.SetOrganizationId(Guid.NewGuid());

            // Get the repository from the service provider
            ITicketRepositoryCommands ticketRepositoryCommands = DbScope.ServiceProvider.GetRequiredService<ITicketRepositoryCommands>();

            var ticket = new Ticket
            {
                Title = "Test Ticket",
                CustomerId = Guid.NewGuid(),
                Status = TicketStatus.Open,
                OrganizationId = currentUser.OrganizationId,
                CreatedAt = DateTime.UtcNow,
            };
            await ticketRepositoryCommands.AddAsync(ticket);
            await ticketRepositoryCommands.SaveChangesAsync();

            // Act
            var result = await ticketRepositoryCommands.GetTicket(ticket.Id);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal("Test Ticket", result.Value.Title);
        }

        [Fact]
        public async Task GetTicket_ShouldReturnFailure_WhenNotExists()
        {
            //Arrange
            using var DbScope = _fixture.DB.CreateAsyncScope();
            ICurrentUser currentUser = DbScope.ServiceProvider.GetRequiredService<ICurrentUser>();
            currentUser.SetOrganizationId(Guid.NewGuid());

            // Get the repository from the service provider
            ITicketRepositoryCommands ticketRepositoryCommands = DbScope.ServiceProvider.GetRequiredService<ITicketRepositoryCommands>();

            //Act
            var result = await ticketRepositoryCommands.GetTicket(9999); //ikke eksisterende ID

            //Assert
            Assert.True(result.IsFailure);
            Assert.Equal("TicketNotFound", result.Error.Code);
        }
    }
}
