using System.ComponentModel.DataAnnotations;
using IdentityService.Sdk.Services.Customer;
using Microsoft.AspNetCore.Components;

namespace CCP.UI.Pages.InviteCustomer
{
    public partial class InviteCustomer : ComponentBase
    {
        [Inject] private ILogger<InviteCustomer> Logger { get; set; } = default!;
        [Inject] private ICustomerService CustomerService { get; set; } = default!;

        private InviteCustomerModel InviteCustomerModel { get; set; } = new InviteCustomerModel();
        private bool isSubmitting = false;
        private string? successMessage = null;
        private string? errorMessage = null;

        private async Task Submit()
        {
            isSubmitting = true;
            successMessage = null;
            errorMessage = null;
            StateHasChanged();

            try
            {
                var result = await CustomerService.InviteCustomer(InviteCustomerModel.Email);

                if (result.IsSuccess)
                {
                    successMessage = $"Invitation sent to {InviteCustomerModel.Email}!";
                    InviteCustomerModel = new InviteCustomerModel();
                }
                else
                {
                    errorMessage = $"Failed to send invitation: {result.Error.Description}";
                    Logger.LogError("Failed to invite customer with email {Email}: {Error}", InviteCustomerModel.Email, result.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error inviting customer with email {Email}", InviteCustomerModel.Email);
                errorMessage = "An unexpected error occurred. Please try again.";
            }
            finally
            {
                isSubmitting = false;
                StateHasChanged();
            }
        }
    }

    public class InviteCustomerModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Welcome message cannot exceed 500 characters")]
        public string? WelcomeMessage { get; set; }
    }
}
