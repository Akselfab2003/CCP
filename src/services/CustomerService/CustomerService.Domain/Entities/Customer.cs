namespace CustomerService.Domain.Entities
{
    public class Customer
    {
        public required Guid Id { get; set; }
        public required Guid OrganizationId { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
    }
}
