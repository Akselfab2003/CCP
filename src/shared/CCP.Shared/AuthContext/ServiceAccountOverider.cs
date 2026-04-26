namespace CCP.Shared.AuthContext
{
    public class ServiceAccountOverrider
    {
        public Guid OrganizationId { get; private set; }

        public string OrganizationName { get; private set; } = string.Empty;

        public void SetOrganizationId(Guid organizationId)
        {
            OrganizationId = organizationId;
        }

        public void SetOrganizationName(string organizationName)
        {
            OrganizationName = organizationName;
        }

    }
}
