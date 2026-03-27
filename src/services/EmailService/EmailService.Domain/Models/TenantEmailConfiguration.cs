namespace EmailService.Domain.Models
{
    public class TenantEmailConfiguration
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public required string Domain { get; set; }
        public string DefaultSenderEmail { get; set; } = string.Empty;
    }
}
