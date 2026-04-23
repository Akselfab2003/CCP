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
        // Base roles matching Keycloak
        public const string AdminRoleString = "org.Admin";
        public const string ManagerRoleString = "org.Manager";
        public const string SupporterRoleString = "org.Supporter";
        public const string CustomerRoleString = "org.Customer";

        // Ticket Management Roles
        public const string CreateTicketsRoleString = "org.CreateTickets";
        public const string ViewAllTicketsRoleString = "org.ViewAllTickets";
        public const string ViewAllCustomerTicketsRoleString = "org.ViewAllCustomerTickets";
        public const string AssignTicketsRoleString = "org.AssignTickets";
        public const string ManageTicketStatusRoleString = "org.ManageTicketStatus";
        public const string CloseTicketsRoleString = "org.CloseTickets";
        public const string ReopenTicketsRoleString = "org.ReopenTickets";
        public const string DeleteTicketsRoleString = "org.DeleteTickets";
        public const string AddInternalNotesRoleString = "org.AddInternalNotes";
        public const string ViewTicketHistoryRoleString = "org.ViewTicketHistory";

        // Communication Roles
        public const string RespondToTicketsRoleString = "org.RespondToTickets";
        public const string EscalateTicketsRoleString = "org.EscalateTickets";

        // User Management Roles
        public const string CreateCustomersRoleString = "org.CreateCustomers";
        public const string ManageCustomersRoleString = "org.ManageCustomers";
        public const string CreateSupportersRoleString = "org.CreateSupporters";
        public const string ManageUsersRoleString = "org.ManageUsers";
        public const string DeactivateUsersRoleString = "org.DeactivateUsers";
        public const string AssignRolesRoleString = "org.AssignRoles";
        public const string ViewUsersRoleString = "org.ViewUsers";

        // System Administration Roles
        public const string ManageTenantRoleString = "org.ManageTenant";
        public const string ViewTenantSettingsRoleString = "org.ViewTenantSettings";
        public const string ConfigureMailRoleString = "org.ConfigureMail";
        public const string ConfigureChatbotRoleString = "org.ConfigureChatbot";
        public const string ViewAuditLogRoleString = "org.ViewAuditLog";

        // Reporting Roles
        public const string ViewDashboardRoleString = "org.ViewDashboard";
        public const string ViewStatisticsRoleString = "org.ViewStatistics";

        // Legacy/Alias roles (bagudkompatibilitet)
        public const string InviteUsersRoleString = "org.CreateSupporters"; // Alias til CreateSupporters
        public const string PromoteUsersRoleString = "org.AssignRoles"; // Alias til AssignRoles
        public const string ViewCustomersRoleString = "org.ViewUsers"; // Alias til ViewUsers
        public const string ManageFaqRoleString = "org.ConfigureChatbot"; // FAQ er del af chatbot config
        public const string ManageOrganizationRoleString = "org.ManageTenant"; // Alias til ManageTenant
        public const string ViewTenantTicketsRoleString = "org.ViewAllTickets"; // Alias
        public const string ViewCustomerTicketsRoleString = "org.ViewAllCustomerTickets"; // Alias

        public static string ToRoleString(this UserRole role)
        {
            return role switch
            {
                UserRole.Admin => "org.Admin",
                UserRole.Manager => "org.Manager",
                UserRole.Supporter => "org.Supporter",
                UserRole.Customer => "org.Customer",
                _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
            };
        }

        public static UserRole? FromRoleString(string roleString)
        {
            return roleString switch
            {
                "org.Admin" => UserRole.Admin,
                "org.Manager" => UserRole.Manager,
                "org.Supporter" => UserRole.Supporter,
                "org.Customer" => UserRole.Customer,
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
