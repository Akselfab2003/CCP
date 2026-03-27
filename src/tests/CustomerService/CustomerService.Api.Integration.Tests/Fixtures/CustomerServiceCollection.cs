namespace Customer.Api.Integration.Tests.Fixtures
{
    [CollectionDefinition("CustomerApiCollection")]
    [Trait("Category", "Integration")]
    public class CustomerServiceCollection : ICollectionFixture<CustomerServiceFixture>
    {
    }
}
