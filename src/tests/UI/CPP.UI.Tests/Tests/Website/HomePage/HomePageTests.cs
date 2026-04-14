using CCP.Website.Services;
using CPP.UI.Tests.Fixtures.Website;
using NSubstitute;

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
            var mock = _fixture.Factory.SetMock<IWebsiteReferencesService>();

            mock.Login.Returns("/login");
            mock.Register.Returns("/register");
            var page = await _fixture.CreatePageAsync();
            await page.GotoAsync("/");
            Assert.NotNull(page);
            _output.WriteLine($"Navigating to: {page.Url}");

            // Assert that the main elements of the home page are visible

            var loginLink = page.GetByTestId("login-link");
            var signupLink = page.GetByTestId("signup-link");

            Assert.True(await loginLink.IsVisibleAsync(), "Login link should be visible on the home page.");
            Assert.True(await signupLink.IsVisibleAsync(), "Sign up link should be visible on the home page.");

            Assert.Equal("/login", await loginLink.GetAttributeAsync("href"));
            Assert.Equal("/register", await signupLink.GetAttributeAsync("href"));
        }
    }
}
