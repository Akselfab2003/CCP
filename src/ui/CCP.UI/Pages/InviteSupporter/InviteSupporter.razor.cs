using System.ComponentModel.DataAnnotations;
using IdentityService.Sdk.Services.Supporter;
using Microsoft.AspNetCore.Components;

namespace CCP.UI.Pages.InviteSupporter
{
    public partial class InviteSupporter : ComponentBase
    {
        [Inject] private ISupporterService SupporterService { get; set; } = default!;
        [Inject] private ILogger<InviteSupporter> Logger { get; set; } = default!;

        private InviteSupporterModel InviteSupporterModel { get; set; } = new InviteSupporterModel();

        private bool isSubmitting = false;
        private string? successMessage = null;
        private string? submitErrorMessage = null;

        private async Task Submit()
        {
            isSubmitting = true;
            successMessage = null;
            submitErrorMessage = null;
            StateHasChanged(); // Force UI update

            try
            {
                var result = await SupporterService.InviteSupporter(InviteSupporterModel.Email);

                if (result.IsSuccess)
                {
                    successMessage = $"Invitation sent to {InviteSupporterModel.Email}!";

                    // Reset form
                    InviteSupporterModel = new InviteSupporterModel();
                }
                else
                {
                    submitErrorMessage = $"Failed to send invitation: {result.Error.Description}";
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "💥 Exception in Submit: {Message}", ex.Message);
                submitErrorMessage = "An unexpected error occurred. Please try again.";
            }
            finally
            {
                isSubmitting = false;
                StateHasChanged(); // Force UI update
            }
        }

        private void OnInvalidSubmit()
        {
            submitErrorMessage = "Please fill in all required fields correctly.";
            StateHasChanged();
        }
    }

    // Model til form data
    public class InviteSupporterModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Welcome message cannot exceed 500 characters")]
        public string? WelcomeMessage { get; set; }
    }
}
