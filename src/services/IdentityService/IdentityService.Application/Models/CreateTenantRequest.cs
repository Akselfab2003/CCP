namespace IdentityService.Application.Models
{
    public class CreateTenantRequest
    {
        public required string OrganizationName { get; set; }
        public required string DomainName { get; set; }
        public required CreateAdminUserRequest AdminUser { get; set; }
    }

    public class CreateAdminUserRequest
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
