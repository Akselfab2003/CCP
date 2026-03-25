using System.ComponentModel.DataAnnotations;

namespace CCP.Website.Components.Register
{
    public class RegisterOrganizationModel
    {

        [Required]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        public string Domain { get; set; } = string.Empty;
    }
}
