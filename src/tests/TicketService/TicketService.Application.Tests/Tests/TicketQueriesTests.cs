using CCP.Shared.ResultAbstraction;
using CCP.Shared.ValueObjects;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TicketService.Application.Services.Ticket;
using TicketService.Domain.Interfaces;
using TicketService.Domain.ResponseObjects;

namespace TicketService.Application.Tests.Tests
{
    public class TicketQueriesTests
    {
        //Dependencies
        private readonly ILogger<TicketQueries> _logger;
        private readonly ITicketRepositoryQueries _ticketRepositoryQueries;
        private readonly TicketQueries _sut; //System Under Test

        public TicketQueriesTests()
        {
            //Setup mocks i constructor
            _logger = Substitute.For<ILogger<TicketQueries>>();
            _ticketRepositoryQueries = Substitute.For<ITicketRepositoryQueries>();

            //Lav System Under Test med mocked dependencies
            _sut = new TicketQueries(_logger, _ticketRepositoryQueries);
        }

        [Fact]
        public async Task GetTicket_ShouldReturnSuccess_WhenTicketExists()
        {
            //Arrange
            var ticketId = 1;
            var mockTicketDto = new TicketDto
            {
                Id = ticketId,
                Title = "Test Ticket",
                CustomerId = Guid.NewGuid(),
                OrganizationId = Guid.NewGuid(),
                Status = TicketStatus.Open,
                CreatedAt = DateTime.UtcNow
            };

            //Mock at repository returnerer succesfuldt
            _ticketRepositoryQueries.GetTicket(ticketId, null).Returns(Result.Success(mockTicketDto));

            //Act
            var result = await _sut.GetTicket(ticketId);

            //Assert
            Assert.NotNull(result.Value);
            Assert.Equal(ticketId, result.Value.Id);
            Assert.Equal("Test Ticket", result.Value.Title);

            //Verificer at repository blev kaldt med de rigtige parametre
            await _ticketRepositoryQueries.Received(1).GetTicket(ticketId, null);
        }

        [Fact]
        public async Task GetTicket_ShouldReturnFailure_WhenTicketNotFound()
        {
            //Assert
            var ticketId = 999; //findes ikke
            var notFoundError = Error.Failure(code: "NotFound", description: "Ticket not found");

            //Mock at repository returnerer failure
            _ticketRepositoryQueries.GetTicket(ticketId, null).Returns(Result.Failure<TicketDto>(notFoundError));

            //Act
            var result = await _sut.GetTicket(ticketId);

            //Assert
            Assert.True(result.IsFailure);
            Assert.Equal("NotFound", result.Error.Code);

            //Verificer at repository blev kaldt
            await _ticketRepositoryQueries.Received(1).GetTicket(ticketId, null);
        }

        [Fact]
        public async Task GetTicket_ShouldReturnFailure_WhenExceptionOccurs()
        {
            //Arrange
            var ticketId = 1;
            var exception = new Exception("Database error");

            //Mock at repository kaster en exception
            _ticketRepositoryQueries.GetTicket(ticketId, null).ThrowsAsync(exception);

            //Act
            var result = await _sut.GetTicket(ticketId);

            //Assert
            Assert.True(result.IsFailure);
            Assert.Contains("An error occurred while retrieving the ticket", result.Error.Description);
            Assert.Contains("1", result.Error.Description);

            //Verificer at repository blev kaldt
            await _ticketRepositoryQueries.Received(1).GetTicket(ticketId, null);
        }

        [Fact]
        public async Task GetTicketsBasedOnParameters_ShouldReturnSuccess_WithFilters()
        {
            //Arrange
            var customerId = Guid.NewGuid();
            var mockTickets = new List<TicketDto>
            {
                new TicketDto
                {
                    Id = 1,
                    Title = "Ticket 1",
                    CustomerId = customerId,
                    Status = TicketStatus.Open
                },
                new TicketDto
                {
                    Id = 2,
                    Title = "Ticket 2",
                    CustomerId = customerId,
                    Status = TicketStatus.Open
                }
            };

            //Mock at repository returnerer listen
            _ticketRepositoryQueries.GetTicketsBasedOnParameters(null, customerId, null).Returns(Result.Success(mockTickets));

            //Act
            var result = await _sut.GetTicketsBasedOnParameters(null, customerId, null);

            //Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(2, result.Value.Count);
            //Begge tickets skal have den rigtige CustomerId
            Assert.All(result.Value, t => Assert.Equal(customerId, t.CustomerId));

            //Verificer at repository blev kaldt med de rigtige parametre
            await _ticketRepositoryQueries.Received(1).GetTicketsBasedOnParameters(null, customerId, null);
        }

        [Fact]
        public async Task GetTicketsBasedOnParameters_ShouldReturnEmptyList_WhenNoMatches()
        {
            //Arrange
            var customerId = Guid.NewGuid();
            var emptyList = new List<TicketDto>();

            //Mock at repository returnerer tom liste
            _ticketRepositoryQueries.GetTicketsBasedOnParameters(null, customerId, null).Returns(Result.Success(emptyList));

            //Act
            var result = await _sut.GetTicketsBasedOnParameters(null, customerId, null);

            //Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Empty(result.Value);

            //Verificer at repository blev kaldt
            await _ticketRepositoryQueries.Received(1).GetTicketsBasedOnParameters(null, customerId, null);
        }

        [Fact]
        public async Task GetTicketsBasedOnParameters_ShouldReturnFailure_WhenExceptionThrown()
        {
            //Arrange
            var customerId = Guid.NewGuid();

            //Mock at repository returnerer tom liste
            _ticketRepositoryQueries.GetTicketsBasedOnParameters(null, customerId, null).ThrowsAsync(new Exception("Database timeout"));

            //Act
            var result = await _sut.GetTicketsBasedOnParameters(null, customerId, null);

            //Assert
            Assert.True(result.IsFailure);
            Assert.Equal("TicketRetrievalFailed", result.Error.Code);
            //Beskrivelsen skal indeholde information om fejlen
            Assert.Contains("An error occurred while retrieving tickets", result.Error.Description);
        }
    }
}
