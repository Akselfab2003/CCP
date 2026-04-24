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
        public bool EmailSaved { get; set; }
        public bool IsSaving { get; set; }

        public async Task SaveEmailConfig()
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                return;
            }

            try
            {
                IsSaving = true;
                await EmailSdkService.CreateTenantEmailAsync(Email);
                EmailSaved = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                IsSaving = false;
            }
        }
    }
}
