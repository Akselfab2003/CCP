namespace TicketService.Api.IntegrationTests.Fixtures
{
    [CollectionDefinition("TicketService")]
    //[Trait("Category", "Integration")] Since it fails currently in ui 
    public class TicketServiceCollection : ICollectionFixture<TicketServiceFixture>
    {
    }
}
