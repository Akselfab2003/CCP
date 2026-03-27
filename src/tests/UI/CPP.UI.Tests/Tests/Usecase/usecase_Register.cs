using System.Text.RegularExpressions;
using CPP.UI.Tests.Attributes;
using CPP.UI.Tests.Fixtures;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit.v3;
using TestUtils.UI;

namespace CPP.UI.Tests.Tests.Usecase
{
    [Collection("UI")]
    [WithTestNameAttribute]
    public class usecase_Register : PageTest
    {
        private readonly UIServiceFixture _fixture;
        private readonly ITestOutputHelper _outputHelper;
        private float _defaultTimeout = (float)TimeSpan.FromMinutes(1).TotalMilliseconds;

        public usecase_Register(UIServiceFixture fixture, ITestOutputHelper outputHelper)
        {
            _fixture = fixture;
            _outputHelper = outputHelper;
        }

        public override async ValueTask InitializeAsync()
        {
            await base.InitializeAsync().ConfigureAwait(false);
            Context.Setup($"{WithTestNameAttribute.CurrentTestClassName}.{WithTestNameAttribute.CurrentTestName}");
        }

        public override async ValueTask DisposeAsync()
        {
            await Context.TeardownAsync($"{WithTestNameAttribute.CurrentTestClassName}.{WithTestNameAttribute.CurrentTestName}");
        }

        [Fact]
        public async Task Valid_Register_Submission_Should_Redirect_To_Login_Page()
        {
            await Page.GotoAsync(_fixture.WebsiteEndpoint);
            await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "CCP" })).ToBeVisibleAsync();

            await Page.GetByRole(AriaRole.Link, new() { Name = "Sign up" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Company name *" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Company name *" }).FillAsync("test");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Domain *" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Domain *" }).FillAsync("test.com");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Next" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "First name *" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "First name *" }).FillAsync("test");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Last name *" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Last name *" }).FillAsync("test");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email *" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Email *" }).FillAsync("test@test.com");
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password *" }).ClickAsync();
            await Page.GetByRole(AriaRole.Textbox, new() { Name = "Password *" }).FillAsync("testtest");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Done" }).ClickAsync();

            await Expect(Page).ToHaveURLAsync(new Regex(".*/realms/CCP/.*"), new() { Timeout = _defaultTimeout });
        }

        [Fact(Skip = "test")]
        public async Task Invalid_Email_Register_Submission_Should_Not_Redirect()
        {
            // Arrange
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

            await Page.GotoAsync(_fixture.WebsiteEndpoint);
            await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "CCP" })).ToBeVisibleAsync();

            await Page.GetByRole(AriaRole.Link, new() { Name = "Sign up" }).ClickAsync();

            await FillOutCompanyDetailsForm(formSubmission);
            await FillOutAccountDetailsForm(formSubmission);

            // Assert
            await Expect(Page).Not.ToHaveURLAsync(new Regex(".*/realms/CCP/.*"), new() { Timeout = _defaultTimeout });
            await Expect(Page.GetByTestId("email-input").First).ToHaveClassAsync(new Regex(".*invalid.*"), new LocatorAssertionsToHaveClassOptions()
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
            var ct = TestContext.Current.CancellationToken;

            // Act

            await Page.GotoAsync(_fixture.WebsiteEndpoint);
            await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "CCP" })).ToBeVisibleAsync();

            await Page.GetByRole(AriaRole.Link, new() { Name = "Sign up" }).ClickAsync();

            await FillOutCompanyDetailsForm(formValue);
            await FillOutAccountDetailsForm(formValue);

            // Assert
            await Expect(Page).Not.ToHaveURLAsync(new Regex(".*/realms/CCP/.*"), new() { Timeout = _defaultTimeout });
            await Expect(Page.GetByTestId(testid).First).ToHaveClassAsync(new Regex(".*invalid.*"), new LocatorAssertionsToHaveClassOptions()
            {
                Timeout = _defaultTimeout
            });
        }



        private async Task FillOutCompanyDetailsForm(CustomerFormSubmission form)
        {
            await Page.GetByTestId("company-name-input").FillAsync(form.OrganizationName);
            await Page.GetByTestId("domain-name-input").FillAsync(form.Domain);
            await Page.GetByRole(AriaRole.Button, new() { Name = "Next" }).ClickAsync();
        }
        private async Task FillOutAccountDetailsForm(CustomerFormSubmission form)
        {
            await Page.GetByTestId("first-name-input").FillAsync(form.FirstName);
            await Page.GetByTestId("last-name-input").FillAsync(form.LastName);
            await Page.GetByTestId("email-input").FillAsync(form.Email);
            await Page.GetByTestId("password-input").FillAsync(form.Password);
            await Page.GetByRole(AriaRole.Button, new() { Name = "Done" }).ClickAsync();
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
