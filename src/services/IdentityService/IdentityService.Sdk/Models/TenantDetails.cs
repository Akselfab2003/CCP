namespace IdentityService.Sdk.Models
{
    public class TenantDetails
    {
        public Guid OrgId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DomainName { get; set; } = string.Empty;
    }
}
