namespace Keycloak.Sdk.Models
{
    public class UserKeycloakAccount
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool? Enabled { get; set; }
        public long? CreatedTimestamp { get; set; }
        public List<string>? RealmRoles { get; set; }
        public List<string>? Groups { get; set; }
    }
}
