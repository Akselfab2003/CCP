using TicketService.Api.IntegrationTests.Fixtures;

namespace TicketService.Api.IntegrationTests.Tests
{
    [Collection("TicketService")]
    public class Example
    {
        private readonly TicketServiceFixture _fixture;

        public Example(TicketServiceFixture fixture)
        {
            _fixture = fixture;
        }
    }
}
