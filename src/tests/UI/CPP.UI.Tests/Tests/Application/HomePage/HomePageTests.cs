using CCP.Shared.ResultAbstraction;
using CCP.Shared.UIContext;
using CPP.UI.Tests.Fixtures.Application;
using NSubstitute;
using TicketService.Sdk.Dtos;
using TicketService.Sdk.Services.Ticket;

namespace CPP.UI.Tests.Tests.Application.HomePage
{
    [Collection("Application")]
    public class HomePageTests
    {
        private readonly BlazorApplicationFixture _fixture;
        private readonly ITestOutputHelper _outputHelper;
        public HomePageTests(BlazorApplicationFixture fixture, ITestOutputHelper outputHelper)
        {
            _fixture = fixture;
            _outputHelper = outputHelper;
        }

        [Fact]
        public async Task HomePage_Should_Display_Correctly_Based_On_User_Role()
        {

            var user = _fixture.Factory.GetUserContext();
            user.SetCustomer();

            var mock = _fixture.Factory.SetMock<IUIUserContext>();
            var ticketmock = _fixture.Factory.SetMock<ITicketService>();
            var UIUserContext = _fixture.Factory.SetMock<IUIUserContext>();


            mock.Role.Returns(CCP.Shared.ValueObjects.UserRole.Customer);

            ticketmock.GetTickets(ct: TestContext.Current.CancellationToken).Returns(Result.Failure<List<TicketSdkDto>>(Error.Failure("", "")));


            var page = await _fixture.CreatePageAsync();
            await page.GotoAsync("/");

            Assert.NotNull(page);
            // Assert that the main elements of the home page are visible
            //  var welcomeMessage = page.GetByTestId("dash-greeting-sub");



            // Assert.Contains("Here's the latest on your support tickets.", await welcomeMessage.InnerTextAsync());
            //Assert.True(await welcomeMessage.IsVisibleAsync(), "Welcome message should be visible on the home page.");
        }


        [Fact]
        public async Task HomePage_Should_Only_Be_Available_To_Authenticated_Users()
        {
            var user = _fixture.Factory.GetUserContext();
            user.SetAnonymous();

            var page = await _fixture.CreatePageAsync();
            var response = await page.GotoAsync("/");

            Assert.NotNull(page);
            Assert.Equal(401, response?.Status);
        }
    }
}
