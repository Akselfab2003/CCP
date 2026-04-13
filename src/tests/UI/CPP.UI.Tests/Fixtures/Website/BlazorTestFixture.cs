using Microsoft.Playwright;

namespace CPP.UI.Tests.Fixtures.Website
{
    public class BlazorTestFixture : IAsyncLifetime
    {
        public IBrowser Browser { get; private set; }
        public TestFactory TestFactory { get; private set; }

        private IPlaywright _playwright;

        public ValueTask InitializeAsync()
        {
            throw new NotImplementedException();
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
}
