namespace IdentityService.Application.Models
{
    public class AssignUserRightsRequest
    {
        public required Guid UserId { get; set; }
        public required string Role { get; set; }
    }
}
