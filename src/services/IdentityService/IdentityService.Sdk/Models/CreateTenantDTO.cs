using System.ComponentModel.DataAnnotations;

namespace IdentityService.Sdk.Models
{
    public class CreateTenantDTO
    {
        [Required(ErrorMessage = "Organization name is required")]
        public required string OrganizationName { get; set; }
        [Required(ErrorMessage = "Domain name is required")]
        public required string DomainName { get; set; }
        public required CreateAdminUserDTO AdminUser { get; set; }
    }

    public class CreateAdminUserDTO
    {
        [Required(ErrorMessage = "First name is required")]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public required string Password { get; set; }
    }
}
