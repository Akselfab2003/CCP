namespace IdentityService.Application.Models
{
    public class AuthenticatingRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
