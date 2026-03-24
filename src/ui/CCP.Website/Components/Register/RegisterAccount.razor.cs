using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace CCP.Website.Components.Register
{
    public partial class RegisterAccount : ComponentBase
    {

        private EditContext? editContext;

        [SupplyParameterFromForm]
        private RegisterAccountModel? Model { get; set; }

        [Parameter]
        public EventCallback<RegisterAccountModel> OnValidSubmit { get; set; }


        private string passwordValid = "form-input ";
        private string PasswordStrengthLabel = "Password strength";
        private string PasswordStrengthCssClass = "pw-strength";

        protected override void OnInitialized()
        {
            Model ??= new();
            editContext = new EditContext(Model);
        }

        private void PasswordUpdated(ChangeEventArgs e)
        {
            if (e.Value == null)
            {
                PasswordStrengthUpdate(string.Empty);
                return;
            }
            else
            {
                PasswordStrengthUpdate(e.Value!.ToString());
            }
        }

        private void PasswordStrengthUpdate(string? updatedValue)
        {

            if (string.IsNullOrEmpty(updatedValue))
            {
                PasswordStrengthLabel = "Password strength";
                PasswordStrengthCssClass = "pw-strength";
                passwordValid = "form-input error";
                return;
            }

            Model!.Password = updatedValue;
            passwordValid = "form-input success";

            var strength = getPasswordStrength(updatedValue ?? string.Empty);
            PasswordStrengthCssClass = $"pw-strength pw-strength-{strength}";

            switch (strength)
            {
                case 0:
                    PasswordStrengthLabel = "Vary weak password";
                    break;
                case 1:
                    PasswordStrengthLabel = "Weak password";
                    break;
                case 2:
                    PasswordStrengthLabel = "Fair password";
                    break;
                case 3:
                    PasswordStrengthLabel = "Good password";
                    break;
                default:
                    PasswordStrengthLabel = "Strong password";
                    break;
            }
        }

        private int getPasswordStrength(string Password)
        {
            int strength = 0;
            if (Password.Length >= 8)
                strength++;
            if (Password.Length >= 12)
                strength++;
            if (System.Text.RegularExpressions.Regex.IsMatch(Password, @"[a-z]") && System.Text.RegularExpressions.Regex.IsMatch(Password, @"[0-9]"))
                strength++;
            if (System.Text.RegularExpressions.Regex.IsMatch(Password, @"[^A-Za-z0-9]"))
                strength++;
            return strength;
        }

        public async Task HandleValidSubmit()
        {
            await OnValidSubmit.InvokeAsync(Model!);
        }
    }
}
