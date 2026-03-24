namespace CCP.Shared.AuthContext
{
    public class CurrentUser : ICurrentUser
    {
        public Guid UserId { get; private set; }

        public Guid OrganizationId { get; private set; }

        public void SetCurrentUser(Guid userId)
        {
            UserId = userId;
        }

        public void SetOrganizationId(Guid organizationId)
        {
            OrganizationId = organizationId;
        }
    }
}
