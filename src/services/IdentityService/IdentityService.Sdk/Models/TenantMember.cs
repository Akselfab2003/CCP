namespace IdentityService.Sdk.Models
{
    public class TenantMember
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = [];
        public List<string> Groups { get; set; } = [];
    }
}
