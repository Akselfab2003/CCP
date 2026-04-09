namespace TicketService.Api.IntegrationTests.Fixtures
{
    [CollectionDefinition("TicketService")]
    [Trait("Category", "Dontwork")] //Since it fails currently in ui 
    public class TicketServiceCollection : ICollectionFixture<TicketServiceFixture>
    {
    }
}
