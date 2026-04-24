namespace IdentityService.API.Endpoints
{
    public class TenantInfoDto
    {
        public required string OrgName { get; set; }
        public required string DomainName { get; set; }
        public required Guid TenantId { get; set; }
    }
}
