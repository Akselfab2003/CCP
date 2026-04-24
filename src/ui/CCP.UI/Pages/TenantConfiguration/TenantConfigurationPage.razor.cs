using EmailService.Sdk.Services;
using Microsoft.AspNetCore.Components;

namespace CCP.UI.Pages.TenantConfiguration
{
    public partial class TenantConfigurationPage
    {
        [Inject]
        public IEmailSdkService emailSdkService { get; set; } = null!;

        public required string Email { get; set; }

        public async Task SaveEmailConfig()
        {
            try
            {
                await emailSdkService.CreateTenantEmailAsync(Email);
            }
            catch (Exception)
            {
                
            }


        }
    }
}
