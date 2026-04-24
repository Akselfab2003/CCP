using System.Text.RegularExpressions;
using CCP.Shared.ResultAbstraction;
using CCP.Website.Services;
using CPP.UI.Tests.Fixtures.Website;
using IdentityService.Sdk.Models;
using IdentityService.Sdk.Services.Tenant;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit.v3;
using NSubstitute;


namespace CPP.UI.Tests.Tests.Website.RegisterPage
{
    [Collection("Website")]
    public class RegisterPageTests : PlaywrightTest
    {
        private readonly BlazorTestFixture _fixture;
        private ITestOutputHelper _output;
        private float _defaultTimeout = (float)TimeSpan.FromSeconds(30).TotalMilliseconds;

        public RegisterPageTests(BlazorTestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
        }

        private void setupMocks()
        {
            var mock = _fixture.Factory.SetMock<IWebsiteReferencesService>();
            mock.Login.Returns("/login");
            mock.Register.Returns("/register");

        }

        [Fact]
        public async Task Valid_Register_Submission_Should_Redirect_To_Login_Page()
        {
            setupMocks();
            var ITenantServiceMock = _fixture.Factory.SetMock<ITenantService>();
            ITenantServiceMock.CreateTenant(Arg.Any<CreateTenantDTO>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(Result.Success()));
            var page = await _fixture.CreatePageAsync();
            await page.GotoAsync("/");
            await page.GetByRole(AriaRole.Link, new() { Name = "CCP" }).IsVisibleAsync();
            CustomerFormSubmission customerFormSubmission = new CustomerFormSubmission()
            {
                OrganizationName = "test",
                Domain = "test.com",
                FirstName = "test",
                LastName = "test",
                Email = $"test{Guid.NewGuid()}@test.com",
                Password = $"test{Guid.NewGuid()}@test.com",
            };
            await page.GetByRole(AriaRole.Link, new() { Name = "Sign up" }).ClickAsync();

            await FillOutCompanyDetailsForm(page, customerFormSubmission);
            await FillOutAccountDetailsForm(page, customerFormSubmission);

        }

        [Fact]
        public async Task Invalid_Email_Register_Submission_Should_Not_Redirect()
        {
            // Arrange
            setupMocks();
            var ITenantServiceMock = _fixture.Factory.SetMock<ITenantService>();
            ITenantServiceMock.CreateTenant(Arg.Any<CreateTenantDTO>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(Result.Success()));
            var page = await _fixture.CreatePageAsync();

            var ct = TestContext.Current.CancellationToken;
            CustomerFormSubmission formSubmission = new CustomerFormSubmission()
            {
                OrganizationName = "test",
                Domain = "test.com",
                FirstName = "test",
                LastName = "test",
                Email = "invalid-email-format",
                Password = "testtest",
            };

            // Act

            await page.GotoAsync("/");
            await Expect(page.GetByRole(AriaRole.Link, new() { Name = "CCP" })).ToBeVisibleAsync();


            await page.GetByRole(AriaRole.Link, new() { Name = "Sign up" }).ClickAsync();
            await Expect(page).ToHaveURLAsync(new Regex("/register"), new() { Timeout = _defaultTimeout, IgnoreCase = true });

            await FillOutCompanyDetailsForm(page, formSubmission);
            await FillOutAccountDetailsForm(page, formSubmission);

            // Assert
            //await Expect(Page).Not.ToHaveURLAsync(new Regex(".*/realms/CCP/.*"), new() { Timeout = _defaultTimeout });
            await Expect(page.GetByTestId("email-input").First).ToHaveClassAsync(new Regex(".*invalid.*"), new LocatorAssertionsToHaveClassOptions()
            {
                Timeout = _defaultTimeout
            });

        }


        public static IEnumerable<TheoryDataRow<string, CustomerFormSubmission>> GetInvalidFormSubmissions()
        {

            return [
              new TheoryDataRow<string, CustomerFormSubmission>("email-input",
                new CustomerFormSubmission()
                {
                    OrganizationName = "test",
                    Domain = "test.com",
                    FirstName = "test",
                    LastName = "test",
                    Email = "invalid-email-format",
                    Password = "testtest",
                }),
              new TheoryDataRow<string, CustomerFormSubmission>("first-name-input",
                new CustomerFormSubmission()
                {
                    OrganizationName = "test",
                    Domain = "test.com",
                    FirstName = "",
                    LastName = "test",
                    Email = "test@test.com",
                    Password = "testtest",
                }),
              new TheoryDataRow<string, CustomerFormSubmission>("last-name-input",
                new CustomerFormSubmission()
                {
                    OrganizationName = "test",
                    Domain = "test.com",
                    FirstName = "test",
                    LastName = "",
                    Email = "test@test.com",
                    Password = "testtest",
                })
            ];
        }


        [Theory]
        [MemberData(nameof(GetInvalidFormSubmissions))]
        public async Task ValidateThatFormsDoesntSubmit_WhenFieldsAreInvalid(string testid, CustomerFormSubmission formValue)
        {
            // Arrange
            setupMocks();
            var ITenantServiceMock = _fixture.Factory.SetMock<ITenantService>();
            ITenantServiceMock.CreateTenant(Arg.Any<CreateTenantDTO>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(Result.Success()));

            var ct = TestContext.Current.CancellationToken;
            var page = await _fixture.CreatePageAsync();
            // Act

            await page.GotoAsync("/");
            await Expect(page.GetByRole(AriaRole.Link, new() { Name = "CCP" })).ToBeVisibleAsync();

            await page.GetByRole(AriaRole.Link, new() { Name = "Sign up" }).ClickAsync();

            await FillOutCompanyDetailsForm(page, formValue);
            await FillOutAccountDetailsForm(page, formValue);

            // Assert
            //await Expect(Page).Not.ToHaveURLAsync(new Regex(".*/realms/CCP/.*"), new() { Timeout = _defaultTimeout });
            await Expect(page.GetByTestId(testid).First).ToHaveClassAsync(new Regex(".*invalid.*"), new LocatorAssertionsToHaveClassOptions()
            {
                Timeout = _defaultTimeout
            });
        }



        private async Task FillOutCompanyDetailsForm(IPage page, CustomerFormSubmission form)
        {
            await page.GetByTestId("company-name-input").FillAsync(form.OrganizationName);
            await page.GetByTestId("domain-name-input").FillAsync(form.Domain);
            await page.GetByRole(AriaRole.Button, new() { Name = "Next" }).ClickAsync();
        }
        private async Task FillOutAccountDetailsForm(IPage page, CustomerFormSubmission form)
        {
            await page.GetByTestId("first-name-input").FillAsync(form.FirstName);
            await page.GetByTestId("last-name-input").FillAsync(form.LastName);
            await page.GetByTestId("email-input").FillAsync(form.Email);
            await page.GetByTestId("password-input").FillAsync(form.Password);
            await page.GetByRole(AriaRole.Button, new() { Name = "Done" }).ClickAsync();
        }
    }

    public class CustomerFormSubmission
    {
        public string OrganizationName { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}

