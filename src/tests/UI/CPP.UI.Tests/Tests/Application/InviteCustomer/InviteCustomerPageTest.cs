using System;
using System.Collections.Generic;
using System.Text;
using CPP.UI.Tests.Fixtures.Application;
using IdentityService.Sdk.Services.Customer;
using k8s.KubeConfigModels;
using Microsoft.Playwright;
using NSubstitute;

namespace CPP.UI.Tests.Tests.Application.InviteCustomer
{
    [Collection("Application")]
    public class InviteCustomerPageTest
    {
        private readonly BlazorApplicationFixture _fixture;
        private readonly ITestOutputHelper _outputHelper;
        public InviteCustomerPageTest(BlazorApplicationFixture fixture, ITestOutputHelper outputHelper)
        {
            _fixture = fixture;
            _outputHelper = outputHelper;
        }

        [Fact]
        public async Task InviteCustomerPage_Should_Display_Correctly()
        {
            //Arrange
            var user = _fixture.Factory.GetUserContext();
            user.SetAdmin();
            var page = await _fixture.CreatePageAsync();

            //Act
            await page.GotoAsync("/InviteCustomers");

            //Assert
            Assert.NotNull(page);
            // Assert that the main elements of the invite customer page are visible
            var inviteForm = page.GetByTestId("invite-customer-form");
            Assert.True(await inviteForm.IsVisibleAsync(), "Invite customer form should be visible on the invite customer page.");
        }

        [Fact]
        public async Task InviteTest()
        {
            //Arrange
            ICustomerService mockCustomerService = _fixture.Factory.SetMock<ICustomerService>();
            mockCustomerService.InviteCustomer(Arg.Any<string>()).Returns(Result.Success());

            var user = _fixture.Factory.GetUserContext();
            user.SetAdmin();
            var page = await _fixture.CreatePageAsync();

            //Act
            await page.GotoAsync("/InviteCustomers");
            await page.GetByRole(AriaRole.Textbox, new() { Name = "customer@example.com" }).FillAsync("test2@gmail.com");
            await page.GetByRole(AriaRole.Textbox, new() { Name = "Write a welcome message to" }).FillAsync("please join");
            await page.GetByRole(AriaRole.Button, new() { Name = "Send Invitation" }).ClickAsync();

            //Assert
            await mockCustomerService.Received().InviteCustomer("test2@gmail.com");

        }
    }
}
