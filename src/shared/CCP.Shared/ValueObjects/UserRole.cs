namespace CCP.Shared.ValueObjects
{
    public enum UserRole
    {
        Admin,
        Manager,
        Supporter,
        Customer
    }


    public static class UserRolesExtensions
    {
        public const string AdminRoleString = "CCP.Rolesorg.Admin";
        public const string ManagerRoleString = "CCP.Rolesorg.Manager";
        public const string SupporterRoleString = "CCP.Rolesorg.Supporter";
        public const string CustomerRoleString = "CCP.Rolesorg.Customer";

        public static string ToRoleString(this UserRole role)
        {
            return role switch
            {
                UserRole.Admin => "CCP.Rolesorg.Admin",
                UserRole.Manager => "CCP.Rolesorg.Manager",
                UserRole.Supporter => "CCP.Rolesorg.Supporter",
                UserRole.Customer => "CCP.Rolesorg.Customer",
                _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
            };
        }

        public static UserRole? FromRoleString(string roleString)
        {
            return roleString switch
            {
                "CCP.Rolesorg.Admin" => UserRole.Admin,
                "CCP.Rolesorg.Manager" => UserRole.Manager,
                "CCP.Rolesorg.Supporter" => UserRole.Supporter,
                "CCP.Rolesorg.Customer" => UserRole.Customer,
                _ => null
            };
        }

        public static string ToGroupName(this UserRole role)
        {
            return role switch
            {
                UserRole.Admin => "Admins",
                UserRole.Manager => "Managers",
                UserRole.Supporter => "Supporters",
                UserRole.Customer => "Customers",
                _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
            };
        }
    }
}
