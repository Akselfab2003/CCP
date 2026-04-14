using CPP.UI.Tests.Fixtures.Website;

namespace CPP.UI.Tests.Tests.Website.HomePage
{
    [Collection("Website")]
    public class HomePageTests
    {
        private readonly BlazorTestFixture _fixture;
        private ITestOutputHelper _output;
        public HomePageTests(BlazorTestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }

        [Fact]
        public async Task HomePage_Should_Display_Correctly()
        {
            var page = await _fixture.CreatePageAsync();
            await page.GotoAsync("/");
            Assert.NotNull(page);
            _output.WriteLine($"Navigating to: {_fixture.Factory.BaseUrl}");

            // Assert that the main elements of the home page are visible

            var LoginElement = page.Locator("class=ccp-public-login");
            var SignUpElement = page.Locator("class=ccp-public-signup");

            Assert.True(await LoginElement.IsVisibleAsync());
            Assert.True(await SignUpElement.IsVisibleAsync());
        }
    }
}
