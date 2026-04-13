namespace EmailService.Domain.Requests
{
    public class AddTenantEmailConfigurationRequest
    {
        public required string Domain { get; set; }
        public required string DefaultSenderEmail { get; set; }
    }
}
