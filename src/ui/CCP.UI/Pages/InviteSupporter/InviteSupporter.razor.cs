using Microsoft.AspNetCore.Components;

namespace CCP.UI.Pages.InviteSupporter
{
    public partial class InviteSupporter : ComponentBase
    {
        // Model til form
        private InviteSupporterModel InviteSupporterModel { get; set; } = new InviteSupporterModel();

        // Mock data - liste over customers til dropdown
        private List<CustomerDto> customers = new();

        // Mock data - liste over supporters til højre side
        private List<SupporterDto> supporters = new();

        protected override async Task OnInitializedAsync()
        {
            // TODO: Senere skal dette hente data fra ICustomerService og ISupporterService
            // For nu laver vi bare mock data så UI'en virker

            // Mock customers til dropdown
            customers = new List<CustomerDto>
            {
                new CustomerDto { Id = Guid.NewGuid(), Name = "John Doe", Email = "john@example.com" },
                new CustomerDto { Id = Guid.NewGuid(), Name = "Jane Smith", Email = "jane@example.com" },
                new CustomerDto { Id = Guid.NewGuid(), Name = "Bob Johnson", Email = "bob@example.com" },
                new CustomerDto { Id = Guid.NewGuid(), Name = "Alice Williams", Email = "alice@example.com" }
            };

            // Mock supporters til højre side (starter tom)
            supporters = new List<SupporterDto>();

            await Task.CompletedTask;
        }

        private async Task Submit()
        {
            // TODO: Senere skal dette kalde ISupporterService.InviteSupporter()
            // For nu logger vi bare til console

            if (InviteSupporterModel.CustomerId == Guid.Empty)
            {
                Console.WriteLine("⚠️ No customer selected!");
                return;
            }

            var selectedCustomer = customers.FirstOrDefault(c => c.Id == InviteSupporterModel.CustomerId);
            
            if (selectedCustomer != null)
            {
                Console.WriteLine($"✅ Inviting customer: {selectedCustomer.Name} ({selectedCustomer.Email})");
                Console.WriteLine($"📧 Welcome message: {InviteSupporterModel.WelcomeMessage ?? "No message"}");

                // Simuler at customer bliver supporter (flyt til supporters listen)
                supporters.Add(new SupporterDto
                {
                    Id = selectedCustomer.Id,
                    Name = selectedCustomer.Name,
                    Email = selectedCustomer.Email
                });

                // Fjern fra customers listen
                customers.Remove(selectedCustomer);

                // Reset form
                InviteSupporterModel = new InviteSupporterModel();
                
                StateHasChanged();
            }

            await Task.CompletedTask;
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
    public class InviteSupporterModel
    {
        public Guid CustomerId { get; set; }
        public string? WelcomeMessage { get; set; }
    }

    // DTO til customer data (midlertidig - skal senere komme fra IdentityService.Sdk)
    public class CustomerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    // DTO til supporter data (midlertidig - skal senere komme fra IdentityService.Sdk)
    public class SupporterDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
