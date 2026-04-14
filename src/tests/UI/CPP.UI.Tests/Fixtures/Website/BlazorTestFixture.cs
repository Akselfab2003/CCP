using Microsoft.Playwright;

namespace CPP.UI.Tests.Fixtures.Website
{
    public class BlazorTestFixture : IAsyncLifetime
    {
        public IBrowser Browser { get; private set; } = default!;
        public TestFactory Factory { get; private set; } = default!;

        private IPlaywright _playwright = default!;

        public async ValueTask InitializeAsync()
        {
            Factory = new TestFactory();
            Factory.Start();

            _playwright = await Playwright.CreateAsync();

            Browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });
        }

        public async Task<IPage> CreatePageAsync()
        {
            if (Browser == null)
                throw new InvalidOperationException("Browser has not been initialized.");

            if (Factory == null)
                throw new InvalidOperationException("Test factory has not been initialized.");

            var context = await Browser.NewContextAsync(new BrowserNewContextOptions()
            {
                BaseURL = Factory.BaseUrl,
                IgnoreHTTPSErrors = true
            });

            return await context.NewPageAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await Browser.DisposeAsync();
            _playwright.Dispose();
            Factory.Dispose();
        }


    }
}
