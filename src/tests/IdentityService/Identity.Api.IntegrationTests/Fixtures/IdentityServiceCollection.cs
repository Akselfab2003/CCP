namespace Identity.Api.IntegrationTests.Fixtures
{
    [CollectionDefinition("Identity")]
    [Trait("Category", "Integration")]
    public class IdentityServiceCollection : ICollectionFixture<IdentityServiceFixture>
    {
    }
}
