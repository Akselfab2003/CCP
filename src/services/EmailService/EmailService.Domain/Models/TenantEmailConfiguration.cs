namespace EmailService.Domain.Models
{
    public class TenantEmailConfiguration
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public required string InternalEmail { get; set; }
        public required string InternalEmailPassword { get; set; }
        public required string DefaultSenderEmail { get; set; }
    }
}
