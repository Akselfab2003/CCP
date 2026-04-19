using System.Security.Claims;
using CCP.Shared.ValueObjects;

namespace CPP.UI.Tests.Utils
{
    public class TestUserContext
    {
        public ClaimsPrincipal? User { get; set; }

        public void SetUser(params Claim[] claims)
        {
            var identity = new ClaimsIdentity(claims, "Test");
            User = new ClaimsPrincipal(identity);
        }

        public void SetAnonymous()
        {
            User = null;
        }

        public void SetAdmin()
        {
            SetUser(
                new Claim(ClaimTypes.Name, "Test Admin"),
                new Claim(ClaimTypes.Role, UserRole.Admin.ToRoleString())
            );
        }

        public void SetCustomer()
        {
            SetUser(
                new Claim(ClaimTypes.Name, "Test Customer"),
                new Claim(ClaimTypes.Role, UserRole.Customer.ToRoleString())
            );
        }

        public void SetManager()
        {
            SetUser(
                new Claim(ClaimTypes.Name, "Test Manager"),
                new Claim(ClaimTypes.Role, UserRole.Manager.ToRoleString())
            );
        }

        public void SetSupport()
        {
            SetUser(
                new Claim(ClaimTypes.Name, "Test Supporter"),
                new Claim(ClaimTypes.Role, UserRole.Supporter.ToRoleString())
            );
        }
    }
}
