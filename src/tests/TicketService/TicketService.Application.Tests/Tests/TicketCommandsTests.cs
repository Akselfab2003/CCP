using CCP.Shared.AuthContext;
using CCP.Shared.ResultAbstraction;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TicketService.Application.Services.Assignment;
using TicketService.Application.Services.Ticket;
using TicketService.Domain.Entities;
using TicketService.Domain.Interfaces;
using TicketService.Domain.RequestObjects;

namespace TicketService.Application.Tests.Tests
{
    public class TicketCommandsTests
    {
        // Dependencies - alle skal mockes!
        private readonly ILogger<TicketCommands> _logger;
        private readonly ITicketRepositoryCommands _ticketRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IAssignmentCommands _assignmentCommands;
        private readonly TicketCommands _sut; // System Under Test

        public TicketCommandsTests()
        {
            // Setup mocks i constructor - køres før hver test
            _logger = Substitute.For<ILogger<TicketCommands>>();
            _ticketRepository = Substitute.For<ITicketRepositoryCommands>();
            _currentUser = Substitute.For<ICurrentUser>();
            _assignmentCommands = Substitute.For<IAssignmentCommands>();

            // Setup standard værdier
            _currentUser.OrganizationId.Returns(Guid.NewGuid());

            // Lav System Under Test med mocked dependencies
            _sut = new TicketCommands(
                _logger,
                _ticketRepository,
                _currentUser,
                _assignmentCommands
            );
        }

        [Fact]
        public async Task CreateTicketAsync_ShouldSucceed_WhenNoAssignment()
        {
            //Arrange
            var request = new CreateTicketRequest
            {
                Title = "Test Ticket",
                CustomerId = Guid.NewGuid(),
                AssignedUserId = null // Ingen assignment!
            };

            //Setup repository mock til at returnere succes
            var mockTicket = new Ticket
            {
                Id = 1,
                Title = "Test Ticket",
                CustomerId = request.CustomerId,
                OrganizationId = _currentUser.OrganizationId,
                CreatedAt = DateTime.UtcNow
            };
            _ticketRepository.AddAsync(Arg.Any<Ticket>()).Returns(Result.Success(mockTicket));
            _ticketRepository.SaveChangesAsync().Returns(Task.CompletedTask);

            //Act
            var result = await _sut.CreateTicketAsync(request);

            //Assert
            Assert.True(result.IsSuccess);

            //Verificer at repository blev kaldt korrekt
            await _ticketRepository.Received(1).AddAsync(Arg.Any<Ticket>());
            await _ticketRepository.Received(2).SaveChangesAsync(); // Kaldes 2 gange!

            //Verificer at assignment IKKE blev kaldt
            await _assignmentCommands.DidNotReceive().CreateAssignmentAsync(Arg.Any<int>(), Arg.Any<Guid>());
        }

        [Fact]
        public async Task CreateTicketAsync_ShouldSucceed_WhenWithAssignment()
        {
            // Arrange
            var assignedUserId = Guid.NewGuid();
            var request = new CreateTicketRequest
            {
                Title = "Test Ticket With Assignment",
                CustomerId = Guid.NewGuid(),
                AssignedUserId = assignedUserId // MED assignment
            };

            // Setup repository mock til at returnere succes
            var mockTicket = new Ticket
            {
                Id = 1,
                Title = "Test Ticket With Assignment",
                CustomerId = request.CustomerId,
                OrganizationId = _currentUser.OrganizationId,
                CreatedAt = DateTime.UtcNow
            };
            _ticketRepository.AddAsync(Arg.Any<Ticket>()).Returns(Result.Success(mockTicket));
            _ticketRepository.SaveChangesAsync().Returns(Task.CompletedTask);

            // Setup assignment commands til at returnere succes
            var mockAssignmentId = Guid.NewGuid();
            _assignmentCommands.CreateAssignmentAsync(Arg.Any<int>(), Arg.Any<Guid>())
                .Returns(Result.Success(mockAssignmentId));

            // Act
            var result = await _sut.CreateTicketAsync(request);

            // Assert
            Assert.True(result.IsSuccess);

            // Verificer at repository blev kaldt korrekt
            await _ticketRepository.Received(1).AddAsync(Arg.Any<Ticket>());
            await _ticketRepository.Received(2).SaveChangesAsync(); // Stadig 2 gange

            // Verificer at assignment BLEV kaldt denne gang
            await _assignmentCommands.Received(1).CreateAssignmentAsync(mockTicket.Id, assignedUserId);
        }

        [Fact]
        public async Task CreateTicketAsync_ShouldFail_WhenRepositoryFails()
        {
            //Arrange
            var request = new CreateTicketRequest
            {
                Title = "Test Ticket Failure",
                CustomerId = Guid.NewGuid(),
                AssignedUserId = null
            };

            //Laver error
            var repositoryError = Error.Failure(code: "DatabaseError", description: "Could not save to databse");

            //Mock til at returnere failure
            _ticketRepository.AddAsync(Arg.Any<Ticket>()).Returns(Result.Failure<Ticket>(repositoryError));
            _ticketRepository.SaveChangesAsync().Returns(Task.CompletedTask);

            //Act
            var result = await _sut.CreateTicketAsync(request);

            //Assert
            Assert.True(result.IsFailure);
            Assert.Equal("DatabaseError", result.Error.Code);
            Assert.Equal("Could not save to databse", result.Error.Description);

            // 8. Verificer at assignment IKKE blev kaldt (fordi vi fejlede før)
            await _assignmentCommands.DidNotReceive().CreateAssignmentAsync(Arg.Any<int>(), Arg.Any<Guid>());
        }

        [Fact]
        public async Task CreateTicketAsync_ShouldFail_WhenAssignmentCreationFails()
        {
            //Arrange
            var assignedUserId = Guid.NewGuid();
            var request = new CreateTicketRequest
            {
                Title = "Test Ticket Assignment Failure",
                CustomerId = Guid.NewGuid(),
                AssignedUserId = assignedUserId
            };

            //Mock at creation lykkes
            var mockTicket = new Ticket
            {
                Id = 1,
                Title = "Test Ticket Assignment",
                CustomerId = request.CustomerId,
                OrganizationId = _currentUser.OrganizationId,
                CreatedAt = DateTime.UtcNow
            };
            _ticketRepository.AddAsync(Arg.Any<Ticket>()).Returns(Result.Success(mockTicket));
            _ticketRepository.SaveChangesAsync().Returns(Task.CompletedTask);

            //Mock at assignment creation fejler
            var assignmentError = Error.Failure(code: "AssignmentError", description: "Could not create assignment");
            _assignmentCommands.CreateAssignmentAsync(Arg.Any<int>(), Arg.Any<Guid>())
                .Returns(Result.Failure<Guid>(assignmentError));

            //Act
            var result = await _sut.CreateTicketAsync(request);

            //Assert
            Assert.True(result.IsFailure);
            Assert.Equal("AssignmentError", result.Error.Code);
            Assert.Equal("Could not create assignment", result.Error.Description);

            //Verificer at ticket repository blev kaldt (ticket blev lavet)
            await _ticketRepository.Received(1).AddAsync(Arg.Any<Ticket>());
            await _ticketRepository.Received(1).SaveChangesAsync(); // Kun 1 gang fordi vi fejler inden anden SaveChanges

            //Verificer at assignment BLEV forsøgt (men fejlede)
            await _assignmentCommands.Received(1).CreateAssignmentAsync(mockTicket.Id, assignedUserId);
        }

        [Fact]
        public async Task CreateTicketAsync_ShouldReturnFailure_WhenExceptionThrown()
        {
            //Arrange
            var request = new CreateTicketRequest
            {
                Title = "Test Ticket Exception",
                CustomerId = Guid.NewGuid(),
                AssignedUserId = null
            };

            //Mock at repository kaster exception
            _ticketRepository.AddAsync(Arg.Any<Ticket>()).ThrowsAsync(new Exception("Database connection failed"));

            //Act
            var result = await _sut.CreateTicketAsync(request);

            //Assert
            Assert.True(result.IsFailure);
            Assert.Equal("TicketCreationFailed", result.Error.Code);

            //SaveChangesAsync skal ALDRIG kaldes (fordi exception sker før)
            await _ticketRepository.DidNotReceive().SaveChangesAsync();

            //Assignment skal ALDRIG kaldes (fordi exception sker før)
            await _assignmentCommands.DidNotReceive().CreateAssignmentAsync(Arg.Any<int>(), Arg.Any<Guid>());
        }
    }
}
