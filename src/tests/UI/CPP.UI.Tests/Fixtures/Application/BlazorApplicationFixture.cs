using Microsoft.Playwright;

namespace CPP.UI.Tests.Fixtures.Application
{
    public class BlazorApplicationFixture : IAsyncLifetime
    {
        public IBrowser Browser { get; private set; } = default!;
        public TestFactoryApplication Factory { get; private set; } = default!;

        private IPlaywright _playwright = default!;
        private string? _url = string.Empty;
        public async ValueTask InitializeAsync()
        {
            Factory = new TestFactoryApplication();
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
                IgnoreHTTPSErrors = true,
                RecordVideoDir = Path.Combine(Directory.GetCurrentDirectory(), "playwright-videos"),
            });

            await context.Tracing.StartAsync(new()
            {
                Title = "BlazorTestFixture Trace",
                Screenshots = true,
                Snapshots = true,
                Sources = true,
            });

            return await context.NewPageAsync();
        }

        public async ValueTask DisposeAsync()
        {
            var tracePath = Path.Combine(Directory.GetCurrentDirectory(), "playwright-traces");
            foreach (var context in Browser.Contexts)
            {
                await context.Tracing.StopAsync(new() { Path = Path.Combine(tracePath, $"{Guid.NewGuid()}.zip") });
                await context.CloseAsync();
            }


            await Browser.DisposeAsync();
            _playwright.Dispose();
            Factory.Dispose();
        }

    }
}
