namespace Keycloak.Sdk.Models
{
    internal class KeycloakGroup
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }

        public List<string> Roles { get; set; } = [];

        public Guid ParentId { get; set; }

        public List<KeycloakGroup> SubGroups { get; set; } = [];
    }
}
