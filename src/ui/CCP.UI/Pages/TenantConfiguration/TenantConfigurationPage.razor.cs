using EmailService.Sdk.Services;
using Microsoft.AspNetCore.Components;

namespace CCP.UI.Pages.TenantConfiguration
{
    public partial class TenantConfigurationPage
    {
        [Inject]
        public IEmailSdkService EmailSdkService { get; set; } = null!;

        [Inject]
        public NavigationManager NavigationManager { get; set; } = null!;

        public string Email { get; set; } = string.Empty;
        public bool? ConfigExists { get; set; }
        public bool EmailSaved { get; set; }
        public bool IsSaving { get; set; }
        public bool IsLoading { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                IsLoading = true;
                var existing = await EmailSdkService.GetTenantEmailAsync();
                Console.WriteLine($"Existing config: {existing?.DefaultSenderEmail ?? "null"}");

              if (existing != null)
                {
                    ConfigExists = true;
                    Email = existing.DefaultSenderEmail;
                }
                else
                {
                    ConfigExists = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading tenant configuration: {ex.Message}");
                ConfigExists = false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task SaveEmailConfig()
        {
            if (string.IsNullOrWhiteSpace(Email))
                return;

            try
            {
                IsSaving = true;
                Console.WriteLine($"ConfigExists: {ConfigExists}, Email: {Email}");

                if (ConfigExists == true)
                {
                    Console.WriteLine("Updating existing tenant email configuration");
                    await EmailSdkService.UpdateTenantEmailAsync(Email);
                }
                else
                {
                    Console.WriteLine("Creating new tenant email configuration");
                    await EmailSdkService.CreateTenantEmailAsync(Email);
                }

                ConfigExists = true;
                EmailSaved = true;

                await Task.Delay(10000);
                NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving email configuration: {ex.Message}");
            }
            finally
            {
                IsSaving = false;
            }
        }
    }
}
