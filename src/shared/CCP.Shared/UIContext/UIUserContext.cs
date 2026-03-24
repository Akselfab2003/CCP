using System.Security.Claims;
using CCP.Shared.ValueObjects;
using Microsoft.AspNetCore.Http;

namespace CCP.Shared.UIContext
{
    public class UIUserContext : IUIUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UIUserContext(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;


        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
        public Guid UserId => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value is string userIdStr && Guid.TryParse(userIdStr, out var userId) ? userId : Guid.Empty;
        public string FirstName => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.GivenName)?.Value ?? string.Empty;
        public string LastName => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Surname)?.Value ?? string.Empty;
        public string FullName => _httpContextAccessor.HttpContext?.User?.FindFirst("name")?.Value ?? $"{FirstName} {LastName}".Trim();
        public string Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        public string Initials => BuildInitials(FirstName, LastName);
        public List<string> Roles { get; private set; } = new List<string>();
        public UserRole Role => GetRole();
        public Guid OrganizationId => GetOrganizationId();
        public bool IsInternalUser => Role is UserRole.Admin or UserRole.Manager or UserRole.Supporter;

        private Guid GetOrganizationId()
        {
            var orgClaim = _httpContextAccessor.HttpContext?.User.FindFirst("org");
            if (orgClaim is not null)
            {
                try
                {
                    var orgData = System.Text.Json.JsonSerializer
                        .Deserialize<Dictionary<string, Dictionary<string, string>>>(orgClaim.Value);
                    if (orgData?.First().Value.TryGetValue("id", out var orgId) == true)
                    {
                        return Guid.Parse(orgId);
                    }
                }
                catch
                {
                    return Guid.Empty;
                }
            }

            return OrganizationId;
        }

        private UserRole GetRole()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) throw new InvalidOperationException("No user context available");

            var RolesAndGroups = user.FindAll(ClaimTypes.Role)
                                     .Select(c => c.Value)
                                     .ToList();

            return RolesAndGroups switch
            {
                var roles when roles.Contains(UserRole.Admin.ToRoleString()) => UserRole.Admin,
                var roles when roles.Contains(UserRole.Manager.ToRoleString()) => UserRole.Manager,
                var roles when roles.Contains(UserRole.Supporter.ToRoleString()) => UserRole.Supporter,
                var roles when roles.Contains(UserRole.Customer.ToRoleString()) => UserRole.Customer,
                _ => throw new InvalidOperationException("User does not have a valid role claim")
            };
        }

        private static string BuildInitials(string firstName, string lastName)
        {
            var first = firstName.Length > 0 ? firstName[0].ToString().ToUpper() : "";
            var last = lastName.Length > 0 ? lastName[0].ToString().ToUpper() : "";
            var initials = $"{first}{last}";
            return string.IsNullOrEmpty(initials) ? "?" : initials;
        }
    }
}
