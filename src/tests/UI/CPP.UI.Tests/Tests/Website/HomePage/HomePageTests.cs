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

            var testlogin = await page.GetByTestId("login-link").IsVisibleAsync();
            var testsignup = await page.GetByTestId("signup-link").IsVisibleAsync();

            Assert.True(testlogin, "Login link should be visible on the home page.");
            Assert.True(testsignup, "Sign up link should be visible on the home page.");
        }
    }
}
