using CCP.Shared.AuthContext;
using CCP.Shared.ResultAbstraction;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TicketService.Application.Services.Assignment;
using TicketService.Domain.Entities;
using TicketService.Domain.Interfaces;

namespace TicketService.Application.Tests.Tests
{
    public class AssignmentCommandsTests
    {
        //Dependencies
        private readonly ILogger<AssignmentCommands> _logger;
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly ICurrentUser _currentUser;
        private readonly ITicketRepositoryCommands _ticketRepository;
        private readonly AssignmentCommands _sut; //System Under Test

        //Faste test værdier til genbrug
        private readonly Guid _testUserId;
        private readonly Guid _testOrganizationId;

        public AssignmentCommandsTests()
        {
            //Setup mocks i constructor - køres før hver test
            _logger = Substitute.For<ILogger<AssignmentCommands>>();
            _assignmentRepository = Substitute.For<IAssignmentRepository>();
            _ticketRepository = Substitute.For<ITicketRepositoryCommands>();
            _currentUser = Substitute.For<ICurrentUser>();

            //Setup faste test værdier
            _testUserId = Guid.NewGuid();
            _testOrganizationId = Guid.NewGuid();

            //Setup CurrentUser mock
            _currentUser.UserId.Returns(_testUserId);
            _currentUser.OrganizationId.Returns(_testOrganizationId);

            //Lav System Under Test med mocked dependencies
            _sut = new AssignmentCommands(_logger, _assignmentRepository, _ticketRepository, _currentUser);
        }

        [Fact]
        public async Task CreateAssignmentAsync_ShouldSucceed_WhenRepositorySucceeds()
        {
            //Arrange
            var ticketId = 1;
            var assignUserId = Guid.NewGuid();

            var mockAssignment = new Assignment
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                UserId = assignUserId,
                AssignByUserId = _testUserId,
                UpdatedAt = DateTime.UtcNow
            };

            //Mock at repository returnerer success
            _assignmentRepository.AddAsync(Arg.Any<Assignment>()).Returns(Result.Success(mockAssignment));
            _assignmentRepository.SaveChangesAsync().Returns(Task.CompletedTask);

            //Act
            var result = await _sut.CreateAssignmentAsync(ticketId, assignUserId);

            //Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(mockAssignment.Id, result.Value);

            await _assignmentRepository.Received(1).AddAsync(Arg.Any<Assignment>());
            await _assignmentRepository.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task CreateAssignmentAsync_ShouldFail_WhenRepositoryFails()
        {
            //Arrange
            var ticketId = 1;
            var assignUserId = Guid.NewGuid();

            var repositoryError = Error.Failure(code: "RepositoryError", description: "Failed to add assignment to repository.");

            //Mock at repository returnerer failure
            _assignmentRepository.AddAsync(Arg.Any<Assignment>()).Returns(Result.Failure<Assignment>(repositoryError));

            //Act
            var result = await _sut.CreateAssignmentAsync(ticketId, assignUserId);

            //Assert
            Assert.True(result.IsFailure);
            Assert.Equal("RepositoryError", result.Error.Code);
            Assert.Equal("Failed to add assignment to repository.", result.Error.Description);

            //AddAsync blev kaldt
            await _assignmentRepository.Received(1).AddAsync(Arg.Any<Assignment>());

            //SaveChangesAsync skal ikke kaldes når AddAsync fejler
            await _assignmentRepository.DidNotReceive().SaveChangesAsync();
        }

        [Fact]
        public async Task CreateAssignmentAsync_ShouldReturnFailure_WhenExceptionThrown()
        {
            //Arrange
            var ticketId = 1;
            var assignUserId = Guid.NewGuid();

            //Mock at repository kaster exception
            _assignmentRepository.AddAsync(Arg.Any<Assignment>()).ThrowsAsync(new Exception("Unexpected database error"));

            //Act
            var result = await _sut.CreateAssignmentAsync(ticketId, assignUserId);

            //Assert
            Assert.True(result.IsFailure);
            Assert.Equal("AssignmentCreationFailed", result.Error.Code);
            Assert.Contains("An error occurred while creating the assignment", result.Error.Description);

            //SaveChangesAsync skal ikke kaldes
            await _assignmentRepository.DidNotReceive().SaveChangesAsync();
        }

        [Fact]
        public async Task CreateOrUpdateAssignment_ShouldCreate_WhenAssignmentDoesNotExist()
        {
            //Arrange
            var ticketId = 1;
            var assignUserId = Guid.NewGuid();

            //Mock at assignmet ikke findes
            var notFoundError = Error.Failure(code: "NotFound", description: "No assignment found for the given ticket ID.");
            _assignmentRepository.GetAssignmentByTicketIdAsync(ticketId).Returns(Result.Failure<Assignment>(notFoundError));

            //Mock create flow
            var newAssignment = Guid.NewGuid();
            var mockAssignment = new Assignment
            {
                Id = newAssignment,
                TicketId = ticketId,
                UserId = assignUserId,
                AssignByUserId = _testUserId,
                UpdatedAt = DateTime.UtcNow
            };
            _assignmentRepository.AddAsync(Arg.Any<Assignment>()).Returns(Result.Success(mockAssignment));
            _assignmentRepository.SaveChangesAsync().Returns(Task.CompletedTask);

            //Act
            var result = await _sut.CreateOrUpdateAssignment(ticketId, assignUserId);

            //Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(newAssignment, result.Value);

            //Verificer at GetAssignmentByTicketIdAsync blev kaldt
            await _assignmentRepository.Received(1).GetAssignmentByTicketIdAsync(ticketId);

            //Verificer at vi kaldte AddAsync
            await _assignmentRepository.Received(1).AddAsync(Arg.Any<Assignment>());

            //Verificer at vi ikke kaldte UpdateAsync
            await _assignmentRepository.DidNotReceive().UpdateAsync(Arg.Any<Assignment>());

            //SaveChangesAsync blev kaldt
            await _assignmentRepository.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task CreateOrUpdateAssignment_ShouldUpdate_WhenAssignmentExists()
        {
            //Arrange
            var ticketId = 1;
            var oldAssignUserId = Guid.NewGuid();
            var newAssignUserId = Guid.NewGuid();

            var existingAssignment = new Assignment
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                UserId = oldAssignUserId,
                AssignByUserId = _testUserId,
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            //Mock at assignment FINDES (GetAssignmentByTicketIdAsync returnerer success)
            _assignmentRepository.GetAssignmentByTicketIdAsync(ticketId).Returns(Result.Success(existingAssignment));

            //Mock UPDATE flow
            _assignmentRepository.UpdateAsync(Arg.Any<Assignment>()).Returns(Result.Success(existingAssignment));
            _assignmentRepository.SaveChangesAsync().Returns(Task.CompletedTask);

            //Act
            var result = await _sut.CreateOrUpdateAssignment(ticketId, newAssignUserId);

            //Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(existingAssignment.Id, result.Value);

            //Verificer at GetAssignmentByTicketIdAsync blev kaldt
            await _assignmentRepository.Received(1).GetAssignmentByTicketIdAsync(ticketId);

            //Verificer at vi kaldte UpdateAsync
            await _assignmentRepository.Received(1).UpdateAsync(Arg.Any<Assignment>());

            //Verificer at vi ikke kaldte AddAsync
            await _assignmentRepository.DidNotReceive().AddAsync(Arg.Any<Assignment>());

            //SaveChangesAsync blev kaldt
            await _assignmentRepository.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task CreateOrUpdateAssignment_ShouldFail_WhenUpdateFails()
        {
            //Arrange
            var ticketId = 1;
            var assignUserId = Guid.NewGuid();

            var existingAssignment = new Assignment
            {
                Id = Guid.NewGuid(),
                TicketId = ticketId,
                UserId = assignUserId,
                AssignByUserId = _testUserId,
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            };

            //Mock at assignment findes
            _assignmentRepository.GetAssignmentByTicketIdAsync(ticketId).Returns(Result.Success(existingAssignment));

            //Mock at UPDATE fejler!
            var updateError = Error.Failure(
                code: "UpdateFailed",
                description: "Could not update assignment"
            );
            _assignmentRepository.UpdateAsync(Arg.Any<Assignment>()).Returns(Result.Failure<Assignment>(updateError));
            //Act
            var result = await _sut.CreateOrUpdateAssignment(ticketId, assignUserId);

            //Assert
            Assert.True(result.IsFailure);
            Assert.Equal("UpdateFailed", result.Error.Code);
            Assert.Equal("Could not update assignment", result.Error.Description);

            //Verificer flow
            await _assignmentRepository.Received(1).GetAssignmentByTicketIdAsync(ticketId);
            await _assignmentRepository.Received(1).UpdateAsync(Arg.Any<Assignment>());

            //aveChangesAsync skal ikke kaldes når update fejler
            await _assignmentRepository.DidNotReceive().SaveChangesAsync();
        }

        [Fact]
        public async Task CreateOrUpdateAssignment_ShouldFail_WhenCreateFails()
        {
            //Arrange
            var ticketId = 1;
            var assignUserId = Guid.NewGuid();

            //Mock at assignment IKKE findes
            var notFoundError = Error.Failure(
                code: "AssignmentNotFound",
                description: "Not found"
            );
            _assignmentRepository.GetAssignmentByTicketIdAsync(ticketId).Returns(Result.Failure<Assignment>(notFoundError));

            //Mock at CREATE fejler!
            var createError = Error.Failure(
                code: "CreateFailed",
                description: "Could not create assignment"
            );
            _assignmentRepository.AddAsync(Arg.Any<Assignment>()).Returns(Result.Failure<Assignment>(createError));
            //Act
            var result = await _sut.CreateOrUpdateAssignment(ticketId, assignUserId);

            //Assert
            Assert.True(result.IsFailure);
            Assert.Equal("CreateFailed", result.Error.Code);

            // 3. Verificer flow
            await _assignmentRepository.Received(1).GetAssignmentByTicketIdAsync(ticketId);
            await _assignmentRepository.Received(1).AddAsync(Arg.Any<Assignment>());
            await _assignmentRepository.DidNotReceive().UpdateAsync(Arg.Any<Assignment>());
            await _assignmentRepository.DidNotReceive().SaveChangesAsync();
        }

        [Fact]
        public async Task CreateOrUpdateAssignment_ShouldReturnFailure_WhenExceptionThrown()
        {
            //Arrange
            var ticketId = 1;
            var assignUserId = Guid.NewGuid();

            //Mock at GetAssignmentByTicketIdAsync kaster exception
            _assignmentRepository.GetAssignmentByTicketIdAsync(ticketId).ThrowsAsync(new Exception("Database connection lost"));

            //Act
            var result = await _sut.CreateOrUpdateAssignment(ticketId, assignUserId);

            //Assert
            Assert.True(result.IsFailure);
            Assert.Equal("AssignmentCreationOrUpdateFailed", result.Error.Code);
            Assert.Contains("An error occurred while creating or updating the assignment", result.Error.Description);

            //Verificer at GetAssignmentByTicketIdAsync blev kaldt
            await _assignmentRepository.Received(1).GetAssignmentByTicketIdAsync(ticketId);

            //Ingen andre operationer skal kaldes
            await _assignmentRepository.DidNotReceive().AddAsync(Arg.Any<Assignment>());
            await _assignmentRepository.DidNotReceive().UpdateAsync(Arg.Any<Assignment>());
            await _assignmentRepository.DidNotReceive().SaveChangesAsync();
        }
    }
}
