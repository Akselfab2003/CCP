using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerService.Sdk.Models
{
    // DTO for creating a new customer via SDK
    public class CreateCustomerRequest
    {
        public required Guid Id { get; set; }
        public required Guid OrganizationId { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
    }
}
