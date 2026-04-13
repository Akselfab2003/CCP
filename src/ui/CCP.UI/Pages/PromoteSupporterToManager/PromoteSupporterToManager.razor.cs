using System.ComponentModel.DataAnnotations;
using IdentityService.Sdk.Services.Supporter;
using Microsoft.AspNetCore.Components;

namespace CCP.UI.Pages.PromoteSupporterToManager
{
    public partial class PromoteSupporterToManager : ComponentBase
    {
        [Inject] private ISupporterService SupporterService { get; set; } = default!;
        [Inject] private ILogger<PromoteSupporterToManager> Logger { get; set; } = default!;

        // Model til form
        private PromoteSupporterModel PromoteSupporterModel { get; set; } = new PromoteSupporterModel();

        // Liste over supporters til dropdown
        private List<SupporterDto> supporters = new();

        // Liste over managers til højre side
        private List<ManagerDto> managers = new();

        private bool isSubmitting = false;
        private string? successMessage = null;
        private string? submitErrorMessage = null;

        protected override async Task OnInitializedAsync()
        {
            // Hent supporters
            var supportersResult = await SupporterService.GetAllSupporters();

            if (supportersResult.IsSuccess)
            {
                supporters = supportersResult.Value.Select(s => new SupporterDto
                {
                    Id = s.Id,
                    Name = $"{s.FirstName} {s.LastName}",
                    Email = s.Email
                }).ToList();
            }
            else
            {
                Logger.LogError("Failed to load supporters: {Error}", supportersResult.Error);
            }

            // TODO: Hent managers når API endpoint er klar
            // var managersResult = await ManagerService.GetAllManagers();
            // if (managersResult.IsSuccess) { managers = ... }
        }

        private async Task Submit()
        {
            isSubmitting = true;
            try
            {
                if (PromoteSupporterModel.SupporterId == Guid.Empty)
                {
                    Logger.LogWarning("No supporter selected!");
                    return;
                }

                // TODO: Implementer PromoteToManager API kald når backend er klar
                // var result = await SupporterService.PromoteToManager(PromoteSupporterModel.SupporterId);

                // Simulering for nu:
                var selectedSupporter = supporters.FirstOrDefault(s => s.Id == PromoteSupporterModel.SupporterId);
                if (selectedSupporter != null)
                {
                    successMessage = $"{selectedSupporter.Name} promoted to Manager!";
                    submitErrorMessage = null;

                    // Fjern fra supporters list
                    supporters.Remove(selectedSupporter);

                    // Tilføj til managers list
                    managers.Add(new ManagerDto
                    {
                        Id = selectedSupporter.Id,
                        Name = selectedSupporter.Name,
                        Email = selectedSupporter.Email
                    });

                    // Reset form
                    PromoteSupporterModel = new PromoteSupporterModel();
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error promoting supporter");
                submitErrorMessage = "An unexpected error occurred. Please try again.";
            }
            finally
            {
                isSubmitting = false;
            }
        }

        // Lav initialer fra navn (f.eks. "John Doe" → "JD")
        private string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "?";

            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
                return parts[0][0].ToString().ToUpper();

            return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
        }
    }

    // Model til form data
    public class PromoteSupporterModel
    {
        [Required(ErrorMessage = "Please select a supporter to promote")]
        public Guid SupporterId { get; set; }

        [MaxLength(500, ErrorMessage = "Message cannot exceed 500 characters")]
        public string? WelcomeMessage { get; set; }
    }

    // DTO til supporter data
    public class SupporterDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    // DTO til manager data
    public class ManagerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
