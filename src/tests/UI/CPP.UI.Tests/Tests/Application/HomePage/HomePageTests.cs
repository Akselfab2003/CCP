using CCP.Shared.UIContext;
using CPP.UI.Tests.Fixtures.Application;
using NSubstitute;

namespace CPP.UI.Tests.Tests.Application.HomePage
{
    [Collection("Application")]
    public class HomePageTests
    {
        private readonly BlazorApplicationFixture _fixture;
        public HomePageTests(BlazorApplicationFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task HomePage_Should_Display_Correctly_Based_On_User_Role()
        {
            var mock = _fixture.Factory.SetMock<IUIUserContext>();
            mock.Role.Returns(CCP.Shared.ValueObjects.UserRole.Customer);

            var page = await _fixture.CreatePageAsync();
            await page.GotoAsync("/");

            Assert.NotNull(page);
            // Assert that the main elements of the home page are visible
            var welcomeMessage = page.GetByTestId("welcome-message");

            Assert.Contains("Welcome back", await welcomeMessage.InnerTextAsync());
            Assert.True(await welcomeMessage.IsVisibleAsync(), "Welcome message should be visible on the home page.");
        }
    }
}
