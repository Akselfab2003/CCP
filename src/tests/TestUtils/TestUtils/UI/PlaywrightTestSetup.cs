using Microsoft.Playwright;

namespace TestUtils.UI
{
    public static class PlaywrightTestSetup
    {
        // Default timeout of 120 seconds 
        private const float DefaultTimeoutMs = 60000 * 2;

        public static async void Setup(this IBrowserContext browserContext, string Title)
        {
            browserContext.SetDefaultTimeout(DefaultTimeoutMs);
            browserContext.SetDefaultNavigationTimeout(DefaultTimeoutMs);
            await browserContext.Tracing.StartAsync(new()
            {
                Title = Title,
                Screenshots = true,
                Snapshots = true,
                Sources = true,
            });

            await browserContext.Browser!.NewContextAsync(new BrowserNewContextOptions()
            {
                IgnoreHTTPSErrors = true,
            });
        }


        public static async Task TeardownAsync(this IBrowserContext context, string traceTitle)
        {
            var tracePath = Path.Combine(Directory.GetCurrentDirectory(), "playwright-traces", $"{traceTitle}.zip");
            await context.Tracing.StopAsync(new() { Path = tracePath });
            await context.CloseAsync();
        }
    }
}
