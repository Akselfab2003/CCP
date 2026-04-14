using Microsoft.Playwright;

namespace CPP.UI.Tests.Fixtures.Website
{
    public class BlazorTestFixture : IAsyncLifetime
    {
        public IBrowser Browser { get; private set; } = default!;
        public TestFactory Factory { get; private set; } = default!;

        private IPlaywright _playwright = default!;
        private string? _url = string.Empty;
        public async ValueTask InitializeAsync()
        {
            Factory = new TestFactory();
            Factory.UseKestrel(0);
            var test = Factory.CreateClient();
            _url = test.BaseAddress?.ToString();
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
                BaseURL = _url,
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
